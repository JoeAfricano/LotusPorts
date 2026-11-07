using Lotus.Design.Controls;
using LotusPorts.Catalog;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.Install;

namespace LotusPorts.App;

/// <summary>What a card says about a game right now.</summary>
public sealed record LiveLine(string Text, BadgeTone Tone);

/// <summary>
/// The live state of each game for cards and shelves: playing, downloading, ready, update available, needs attention,
/// file recognized. One subscription to the play service turns port changes into game changes, so each card only
/// redraws when its own game changes.
/// </summary>
public sealed class LiveState
{
    private readonly PlayService play;
    private readonly CatalogService catalog;
    private readonly GameFilesService files;
    private Dictionary<string, string>? portToGame;

    /// <summary>The game id whose state changed (raised on the UI thread).</summary>
    public event Action<string>? GameChanged;

    public LiveState(PlayService play, CatalogService catalog, GameFilesService files)
    {
        this.play = play;
        this.catalog = catalog;
        this.files = files;
        play.Changed += portId => { if (GameOf(portId) is { } gameId) GameChanged?.Invoke(gameId); };
        files.Changed += () => GameChanged?.Invoke("*");
    }

    private string? GameOf(string portId)
    {
        portToGame ??= catalog.Library.Games.SelectMany(g => g.AllPorts.Select(p => (p.Port.Id, g.Id))).GroupBy(x => x.Item1).ToDictionary(x => x.Key, x => x.First().Item2);
        return portToGame.GetValueOrDefault(portId);
    }

    /// <summary>The port of this game installed on the PC, if any (the recommended one first).</summary>
    public Port? InstalledPort(LibraryGame game) => game.AllPorts.Select(p => p.Port).FirstOrDefault(p => play.IsPortInstalled(p.Id));

    public LiveLine LineFor(string gameId)
    {
        if (catalog.Library.Find(gameId) is not { } game) return new(Strings.Get("StateNotInstalled"), BadgeTone.Unknown);
        if (InstalledPort(game) is { } port)
        {
            var s = play.StateOf(port);
            if (s.RunningSince is not null) return new(Strings.Get("LivePlaying"), BadgeTone.Accent);
            if (s.Progress is { } p)
                return new(ProgressText.For(p), BadgeTone.Accent);
            if (s.Installed?.LastLaunchFailed == true) return new(Strings.Get("LiveAttention"), BadgeTone.Warning);
            if (s.UpdateAvailable) return new(Strings.Get("LiveUpdate"), BadgeTone.Accent);
            return new(Strings.Get("LiveReady"), BadgeTone.Ok);
        }
        if (play.Working.FirstOrDefault(w => w.Port.GameId == gameId) is { Port: not null } w)
            return new(Strings.Format("PlayDownloading", (int)Math.Round(w.Progress.Fraction * 100)), BadgeTone.Accent);
        if (files.MatchFor(gameId) is not null) return new(Strings.Get("LiveFileRecognized"), BadgeTone.Ok);
        return new(Strings.Get("StateNotInstalled"), BadgeTone.Unknown);
    }
}
