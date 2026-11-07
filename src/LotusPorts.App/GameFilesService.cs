using LotusPorts.Catalog;
using LotusPorts.Core.GameFiles;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.GameFiles;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace LotusPorts.App;

/// <summary>A file read on this PC and what it was recognized as (empty when no hash matched the catalog).</summary>
/// <summary>FromZip: the .zip the file was copied out of (Info.Path is then Lotus Ports' copy).</summary>
public sealed record KnownGameFile(GameFileInfo Info, IReadOnlyList<GameFileMatch> Matches, string? FromZip = null);

/// <summary>
/// Game files for the whole app: the folders being watched, the background search, and which games have a recognized
/// file. Files are only read; hashes and paths never leave the PC. Changes raise <see cref="Changed"/> on the UI thread.
/// </summary>
public sealed class GameFilesService(GameFileStore store, GameFileScanner scanner, CatalogService catalog, LotusPorts.Infrastructure.AppPaths paths, ILogger<GameFilesService> logger)
{
    private IReadOnlyList<KnownGameFile> files = [];
    private Dictionary<string, GameFileMatch> byGame = [];
    private CancellationTokenSource? scan;
    private DispatcherQueue? ui;

    public event Action? Changed;

    public IReadOnlyList<string> Folders { get; private set; } = [];

    public IReadOnlyList<KnownGameFile> Files => files;

    public bool Scanning => scan is not null;

    public ScanProgress? Progress { get; private set; }

    /// <summary>When the last search finished, or null when none ran in this session.</summary>
    public DateTimeOffset? LastScan { get; private set; }

    public GameFileMatch? MatchFor(string gameId) => byGame.GetValueOrDefault(gameId);

    /// <summary>Loads what was read before (instant), then looks for new or changed files in the background.</summary>
    public async Task InitializeAsync(DispatcherQueue dispatcher)
    {
        ui = dispatcher;
        catalog.HasRecognizedFile = id => byGame.ContainsKey(id);
        Folders = await store.GetFoldersAsync();
        Apply(await store.ListAsync());
        if (Folders.Count > 0) _ = ScanAsync();
    }

    public async Task ScanAsync()
    {
        if (scan is not null || Folders.Count == 0) return;
        scan = new CancellationTokenSource();
        Raise();
        try
        {
            var progress = new Progress<ScanProgress>(p => { Progress = p; Raise(); });
            await scanner.ScanAsync(Folders, progress, scan.Token);
            Apply(await store.ListAsync());
            await ExtractMatchedFromZipsAsync(scan.Token);
            LastScan = DateTimeOffset.Now;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Microsoft.Data.Sqlite.SqliteException)
        {
            logger.LogWarning(ex, "Game file search failed");
        }
        finally
        {
            scan.Dispose();
            scan = null;
            Progress = null;
            Raise();
        }
    }

    public void CancelScan() => scan?.Cancel();

    public async Task AddFolderAsync(string folder)
    {
        if (Folders.Contains(folder, StringComparer.OrdinalIgnoreCase)) return;
        Folders = [.. Folders, folder];
        await store.SetFoldersAsync(Folders);
        Raise();
        await ScanAsync();
    }

    public async Task RemoveFolderAsync(string folder)
    {
        CancelScan();
        Folders = Folders.Where(f => !string.Equals(f, folder, StringComparison.OrdinalIgnoreCase)).ToList();
        await store.SetFoldersAsync(Folders);
        var root = Path.GetFullPath(folder).TrimEnd('\\') + "\\";
        await store.RemoveAsync(files.Select(f => f.Info.Path).Where(p => p.StartsWith(root, StringComparison.OrdinalIgnoreCase)));
        Apply(await store.ListAsync());
        Raise();
    }

    /// <summary>Reads one file the player picked or dropped and says what it is. The file is kept in the list.</summary>
    public async Task<KnownGameFile> IdentifyAsync(string path)
    {
        if (GameFileZip.IsZip(path)) return await IdentifyZipAsync(path);
        var info = await Task.Run(() => GameFileReader.ReadAsync(path));
        await store.SaveAsync(info);
        Apply(await store.ListAsync());
        Raise();
        return new KnownGameFile(info, info.Hashes is { } h ? GameFileMatcher.Match(catalog.Library.Games, h) : []);
    }

    /// <summary>Folder for game files copied out of a .zip (%LOCALAPPDATA%\LotusPorts\game-files).</summary>
    public string ExtractedFolder => Path.Combine(paths.Root, "game-files");

    /// <summary>
    /// A .zip: the game files inside are copied to <see cref="ExtractedFolder"/> and read. Copies that turn out not to be
    /// game files are removed again (they are Lotus Ports' own copies); the .zip itself is never changed.
    /// </summary>
    private async Task<KnownGameFile> IdentifyZipAsync(string zip)
    {
        IReadOnlyList<string> copies;
        try { copies = await Task.Run(() => GameFileZip.ExtractCandidatesAsync(zip, ExtractedFolder, GameFileReader.DefaultMaxHashBytes)); }
        catch (InvalidDataException ex)
        {
            logger.LogInformation(ex, "Not a readable .zip");
            copies = [];
        }
        var found = new List<KnownGameFile>();
        foreach (var copy in copies)
        {
            var info = await Task.Run(() => GameFileReader.ReadAsync(copy));
            var matches = info.Hashes is { } h ? GameFileMatcher.Match(catalog.Library.Games, h) : [];
            // Not a game file after all: Lotus Ports' copy goes away. A game file is kept even when its version is not
            // listed, because the port itself needs the plain file and checks it when the game opens.
            if (info.Format == GameFileFormat.Unknown) { TryDelete(copy); found.Add(new KnownGameFile(info with { Path = zip }, [], zip)); continue; }
            await store.SaveAsync(info);
            found.Add(new KnownGameFile(info, matches, zip));
        }
        Apply(await store.ListAsync());
        Raise();
        var zipInfo = new FileInfo(zip);
        return found.FirstOrDefault(f => f.Matches.Count > 0) ?? found.FirstOrDefault()
            ?? new KnownGameFile(new GameFileInfo(zip, zipInfo.Length, zipInfo.LastWriteTimeUtc, GameFileFormat.Unknown, null, null, null), [], zip);
    }

    /// <summary>
    /// After a search: a game recognized inside a .zip is copied out once (only that file), because ports need the plain
    /// file. Games already available as a plain file are left alone; the .zip is only read.
    /// </summary>
    private async Task ExtractMatchedFromZipsAsync(CancellationToken cancellationToken)
    {
        var plain = files.Where(f => f.FromZip is null).SelectMany(f => f.Matches).Select(m => m.Game.Id).ToHashSet();
        var extracted = false;
        foreach (var file in files.Where(f => f.FromZip is not null && f.Matches.Count > 0 && f.Matches.Any(m => !plain.Contains(m.Game.Id))).ToList())
        {
            try
            {
                var copy = await Task.Run(() => GameFileZip.ExtractEntryAsync(file.Info.Path, ExtractedFolder, cancellationToken), cancellationToken);
                var info = await Task.Run(() => GameFileReader.ReadAsync(copy, cancellationToken: cancellationToken), cancellationToken);
                await store.SaveAsync(info, cancellationToken);
                foreach (var m in file.Matches) plain.Add(m.Game.Id);
                extracted = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                logger.LogInformation(ex, "Could not copy a recognized game out of its .zip");
            }
        }
        if (extracted) Apply(await store.ListAsync(cancellationToken));
    }

    private void TryDelete(string file)
    {
        try { File.Delete(file); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { logger.LogDebug(ex, "Could not remove {File}", file); }
    }

    private void Apply(IReadOnlyList<GameFileInfo> infos)
    {
        var games = catalog.Library.Games;
        // Plain files first: a game found inside a .zip is also kept as an extracted copy, and the copy is what ports use.
        files = infos.Where(i => i.Format != GameFileFormat.Unknown && File.Exists(GameFileZip.ContainerOf(i.Path)))
            .OrderBy(i => GameFileZip.IsInsideZip(i.Path) ? 1 : 0)
            .Select(i => new KnownGameFile(i, i.Hashes is { } h ? GameFileMatcher.Match(games, h) : [], GameFileZip.IsInsideZip(i.Path) ? GameFileZip.ContainerOf(i.Path) : null))
            .ToList();
        byGame = files.SelectMany(f => f.Matches).GroupBy(m => m.Game.Id).ToDictionary(g => g.Key, g => g.First());
    }

    private void Raise()
    {
        if (ui is null || ui.HasThreadAccess) Changed?.Invoke();
        else ui.TryEnqueue(() => Changed?.Invoke());
    }
}
