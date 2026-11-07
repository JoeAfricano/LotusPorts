using System.IO.Compression;

namespace LotusPorts.Core.GameFiles;

/// <summary>
/// Game files the player keeps inside a .zip (the usual way people store their dumps). The .zip is only read: the game
/// files in it are copied out to a folder of Lotus Ports, because ports need the plain file (a .z64, an .iso…).
/// </summary>
public static class GameFileZip
{
    public const int MaxCandidates = 4;

    public static bool IsZip(string path) => string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase);

    /// <summary>Marks a game file read from inside a .zip: "C:\games\x.zip::Game (USA).z64".</summary>
    public const string Separator = "::";

    public static string EntryPath(string zip, string entry) => zip + Separator + entry;

    public static bool IsInsideZip(string path) => path.Contains(Separator, StringComparison.Ordinal);

    /// <summary>The .zip of an entry path (or the path itself for a plain file).</summary>
    public static string ContainerOf(string path) => IsInsideZip(path) ? path[..path.IndexOf(Separator, StringComparison.Ordinal)] : path;

    /// <summary>
    /// Reads the game files inside a .zip without extracting anything: format, N64 header and hashes come straight from
    /// the compressed entries. Each result's path is <see cref="EntryPath"/>; its date is the .zip's.
    /// </summary>
    public static async Task<IReadOnlyList<GameFileInfo>> ReadEntriesAsync(string zip, long maxBytes, CancellationToken cancellationToken = default)
    {
        var modified = File.GetLastWriteTimeUtc(zip);
        using var archive = ZipFile.OpenRead(zip);
        var result = new List<GameFileInfo>();
        foreach (var entry in archive.Entries.Where(e => e.Name.Length > 0 && GameFileReader.IsCandidate(e.Name) && e.Length > 0).Take(MaxCandidates))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var head = new byte[N64Rom.HeaderSize];
            int headLength;
            await using (var s = entry.Open())
                headLength = await s.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
            var order = N64Rom.Detect(head.AsSpan(0, headLength));
            var format = GameFileReader.FormatOf(entry.Name, order);
            N64Header? header = null;
            if (order is { } o)
            {
                var canonical = head.AsSpan(0, headLength).ToArray();
                N64Rom.ToCanonical(canonical, o);
                header = N64Rom.ReadHeader(canonical);
            }
            GameFileHashes? hashes = null;
            if (format != GameFileFormat.Unknown && entry.Length <= maxBytes)
            {
                await using var s = entry.Open();
                hashes = await GameFileReader.HashAsync(s, order, cancellationToken).ConfigureAwait(false);
            }
            result.Add(new GameFileInfo(EntryPath(zip, entry.FullName), entry.Length, modified, format, order, header, hashes));
        }
        return result;
    }

    /// <summary>Copies one entry (named by <see cref="EntryPath"/>) into <paramref name="folder"/>; reuses a copy of the same size.</summary>
    public static async Task<string> ExtractEntryAsync(string entryPath, string folder, CancellationToken cancellationToken = default)
    {
        var zip = ContainerOf(entryPath);
        var name = entryPath[(zip.Length + Separator.Length)..];
        using var archive = ZipFile.OpenRead(zip);
        var entry = archive.GetEntry(name) ?? throw new FileNotFoundException("Entry not found in the .zip.", name);
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, Path.GetFileName(entry.Name));
        if (File.Exists(target) && new FileInfo(target).Length == entry.Length) return target;
        var temp = target + ".part";
        await using (var input = entry.Open())
        await using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
            await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
        File.Move(temp, target, overwrite: true);
        return target;
    }

    /// <summary>
    /// Copies the entries that look like game files (by extension, at most <paramref name="maxBytes"/> each and
    /// <see cref="MaxCandidates"/> in all) into <paramref name="folder"/>, keeping only the file name. A copy that is
    /// already there with the same size is reused. Returns the copied paths (empty: nothing in it looked like a game file).
    /// </summary>
    public static async Task<IReadOnlyList<string>> ExtractCandidatesAsync(string zip, string folder, long maxBytes, CancellationToken cancellationToken = default)
    {
        using var archive = ZipFile.OpenRead(zip);
        var entries = archive.Entries
            .Where(e => e.Name.Length > 0 && GameFileReader.IsCandidate(e.Name) && e.Length > 0 && e.Length <= maxBytes)
            .Take(MaxCandidates)
            .ToList();
        var result = new List<string>();
        if (entries.Count == 0) return result;
        Directory.CreateDirectory(folder);
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Only the file name: folders inside the .zip (or "..") never decide where the copy goes.
            var target = Path.Combine(folder, Path.GetFileName(entry.Name));
            if (File.Exists(target) && new FileInfo(target).Length == entry.Length) { result.Add(target); continue; }
            var temp = target + ".part";
            await using (var input = entry.Open())
            await using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
            File.Move(temp, target, overwrite: true);
            result.Add(target);
        }
        return result;
    }
}
