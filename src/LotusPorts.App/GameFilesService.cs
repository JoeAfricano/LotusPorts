using LotusPorts.Catalog;
using LotusPorts.Core.GameFiles;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.GameFiles;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace LotusPorts.App;

/// <summary>A file read on this PC and what it was recognized as (empty when no hash matched the catalog).</summary>
public sealed record KnownGameFile(GameFileInfo Info, IReadOnlyList<GameFileMatch> Matches);

/// <summary>
/// Game files for the whole app: the folders being watched, the background search, and which games have a recognized
/// file. Files are only read; hashes and paths never leave the PC. Changes raise <see cref="Changed"/> on the UI thread.
/// </summary>
public sealed class GameFilesService(GameFileStore store, GameFileScanner scanner, CatalogService catalog, ILogger<GameFilesService> logger)
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
        var info = await Task.Run(() => GameFileReader.ReadAsync(path));
        await store.SaveAsync(info);
        Apply(await store.ListAsync());
        Raise();
        return new KnownGameFile(info, info.Hashes is { } h ? GameFileMatcher.Match(catalog.Library.Games, h) : []);
    }

    private void Apply(IReadOnlyList<GameFileInfo> infos)
    {
        var games = catalog.Library.Games;
        files = infos.Where(i => i.Format != GameFileFormat.Unknown && File.Exists(i.Path))
            .Select(i => new KnownGameFile(i, i.Hashes is { } h ? GameFileMatcher.Match(games, h) : []))
            .ToList();
        byGame = files.SelectMany(f => f.Matches).GroupBy(m => m.Game.Id).ToDictionary(g => g.Key, g => g.First());
    }

    private void Raise()
    {
        if (ui is null || ui.HasThreadAccess) Changed?.Invoke();
        else ui.TryEnqueue(() => Changed?.Invoke());
    }
}
