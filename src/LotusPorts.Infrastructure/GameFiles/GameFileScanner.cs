using System.Globalization;
using System.Text.Json;
using Dapper;
using LotusPorts.Core.GameFiles;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.GameFiles;

/// <summary>The folders the player told Lotus Ports to look in, and every game file read there (cached by path + size + date).</summary>
public sealed class GameFileStore(SqliteDatabase database, ISettingsStore settings, TimeProvider time)
{
    public const string FoldersKey = "gameFiles.folders";

    public async Task<IReadOnlyList<string>> GetFoldersAsync(CancellationToken cancellationToken = default)
    {
        var json = await settings.GetAsync(FoldersKey, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json)?.Where(Path.IsPathFullyQualified).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? []; }
        catch (JsonException) { return []; }
    }

    public Task SetFoldersAsync(IEnumerable<string> folders, CancellationToken cancellationToken = default) =>
        settings.SetAsync(FoldersKey, JsonSerializer.Serialize(folders.Where(Path.IsPathFullyQualified).Distinct(StringComparer.OrdinalIgnoreCase).ToList()), cancellationToken);

    public async Task<IReadOnlyList<GameFileInfo>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        var rows = await c.QueryAsync<Row>(new CommandDefinition("SELECT * FROM GameFiles ORDER BY Path", cancellationToken: cancellationToken)).ConfigureAwait(false);
        return rows.Select(ToInfo).ToList();
    }

    public async Task SaveAsync(GameFileInfo info, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(info);
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        await c.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO GameFiles (Path, Size, ModifiedUtc, Format, ByteOrder, InternalName, GameCode, RomVersion, Sha1, Md5, Sha256, Crc32, ReadAt)
            VALUES (@Path, @Size, @ModifiedUtc, @Format, @ByteOrder, @InternalName, @GameCode, @RomVersion, @Sha1, @Md5, @Sha256, @Crc32, @ReadAt)
            ON CONFLICT(Path) DO UPDATE SET Size = excluded.Size, ModifiedUtc = excluded.ModifiedUtc, Format = excluded.Format, ByteOrder = excluded.ByteOrder,
              InternalName = excluded.InternalName, GameCode = excluded.GameCode, RomVersion = excluded.RomVersion, Sha1 = excluded.Sha1, Md5 = excluded.Md5,
              Sha256 = excluded.Sha256, Crc32 = excluded.Crc32, ReadAt = excluded.ReadAt
            """,
            new
            {
                info.Path, info.Size, ModifiedUtc = Stamp(info.ModifiedUtc), Format = info.Format.ToString(), ByteOrder = info.ByteOrder?.ToString(),
                InternalName = info.Header?.InternalName, GameCode = info.Header?.GameCode, RomVersion = info.Header?.Version is { } v ? (int?)v : null,
                info.Hashes?.Sha1, info.Hashes?.Md5, info.Hashes?.Sha256, info.Hashes?.Crc32, ReadAt = time.GetUtcNow().ToString("O", CultureInfo.InvariantCulture),
            }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    public async Task RemoveAsync(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        await using var c = await database.OpenAsync(cancellationToken).ConfigureAwait(false);
        foreach (var path in paths)
            await c.ExecuteAsync(new CommandDefinition("DELETE FROM GameFiles WHERE Path = @path", new { path }, cancellationToken: cancellationToken)).ConfigureAwait(false);
    }

    private static string Stamp(DateTime utc) => utc.ToString("O", CultureInfo.InvariantCulture);

    private static GameFileInfo ToInfo(Row r) => new(
        r.Path, r.Size, DateTime.Parse(r.ModifiedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        Enum.TryParse<GameFileFormat>(r.Format, out var f) ? f : GameFileFormat.Unknown,
        Enum.TryParse<N64ByteOrder>(r.ByteOrder, out var o) ? o : null,
        r.InternalName is null ? null : new N64Header(r.InternalName, r.GameCode ?? "", (byte)(r.RomVersion ?? 0)),
        r.Sha1 is null ? null : new GameFileHashes(r.Sha1, r.Md5 ?? "", r.Sha256 ?? "", r.Crc32 ?? ""));

    private sealed class Row
    {
        public string Path { get; set; } = "";
        public long Size { get; set; }
        public string ModifiedUtc { get; set; } = "";
        public string Format { get; set; } = "";
        public string? ByteOrder { get; set; }
        public string? InternalName { get; set; }
        public string? GameCode { get; set; }
        public long? RomVersion { get; set; }
        public string? Sha1 { get; set; }
        public string? Md5 { get; set; }
        public string? Sha256 { get; set; }
        public string? Crc32 { get; set; }
        public string ReadAt { get; set; } = "";
    }
}

public sealed record ScanProgress(int FilesSeen, int FilesRead, string? Current);

public sealed record ScanResult(IReadOnlyList<GameFileInfo> Files, int Read, int FromCache, int Unreadable);

/// <summary>
/// Looks for game files in the chosen folders, in the background with low priority and cancellable. A file already
/// read with the same size and date is not read again. Links and system folders are skipped; nothing is written to the
/// folders. Hashes and paths stay on this PC.
/// </summary>
public sealed class GameFileScanner(GameFileStore store, ILogger<GameFileScanner> logger)
{
    private const int MaxDepth = 8;

    public Task<ScanResult> ScanAsync(IReadOnlyList<string> folders, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folders);
        return Task.Factory.StartNew(() =>
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            return ScanCoreAsync(folders, progress, cancellationToken).GetAwaiter().GetResult();
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async Task<ScanResult> ScanCoreAsync(IReadOnlyList<string> folders, IProgress<ScanProgress>? progress, CancellationToken cancellationToken)
    {
        var cached = (await store.ListAsync(cancellationToken).ConfigureAwait(false)).ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);
        var found = new List<GameFileInfo>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int read = 0, fromCache = 0, unreadable = 0, seen = 0;

        foreach (var file in folders.Where(Directory.Exists).SelectMany(Candidates))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!seenPaths.Add(file.FullName)) continue;
            seen++;
            if (cached.TryGetValue(file.FullName, out var known) && known.Size == file.Length && known.ModifiedUtc == file.LastWriteTimeUtc)
            {
                fromCache++;
                if (known.Format != GameFileFormat.Unknown) found.Add(known);
                continue;
            }
            progress?.Report(new ScanProgress(seen, read, file.Name));
            try
            {
                var info = await GameFileReader.ReadAsync(file.FullName, cancellationToken: cancellationToken).ConfigureAwait(false);
                await store.SaveAsync(info, cancellationToken).ConfigureAwait(false);
                read++;
                if (info.Format != GameFileFormat.Unknown) found.Add(info);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                unreadable++;
                logger.LogDebug(ex, "Game file not readable");
            }
        }

        // Files that were inside the scanned folders and are gone now leave the list.
        var roots = folders.Select(f => Path.GetFullPath(f).TrimEnd('\\') + "\\").ToList();
        var gone = cached.Keys.Where(p => roots.Any(r => p.StartsWith(r, StringComparison.OrdinalIgnoreCase)) && !seenPaths.Contains(p)).ToList();
        if (gone.Count > 0) await store.RemoveAsync(gone, cancellationToken).ConfigureAwait(false);
        progress?.Report(new ScanProgress(seen, read, null));
        return new ScanResult(found, read, fromCache, unreadable);
    }

    /// <summary>Candidate files under a folder: no links, no hidden/system folders, at most a few levels deep.</summary>
    private static IEnumerable<FileInfo> Candidates(string root)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MaxRecursionDepth = MaxDepth,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint | FileAttributes.System | FileAttributes.Hidden | FileAttributes.Offline,
        };
        return new DirectoryInfo(root).EnumerateFiles("*", options).Where(f => GameFileReader.IsCandidate(f.Name));
    }
}
