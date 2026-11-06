using Lotus.Integration.Contracts;
using LotusPorts.Core.Operations;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Integration;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;

namespace LotusPorts.App;

/// <summary>
/// Answers Lotus (section 12.7/12.8): the games installed or favorited, opening and playing them, and the operation
/// history. Anything that changes the PC (installing) is never done from here: the game page opens instead, where the
/// player confirms.
/// </summary>
public sealed class PortsHandler(CatalogService catalog, PlayService play, LotusBridge bridge, IOperationJournal journal) : IPortsHandler
{
    private DispatcherQueue? ui;

    public void Attach(DispatcherQueue dispatcher) => ui = dispatcher;

    public bool IsAllowed(string method) => method == Methods.JournalQuery
        ? bridge.IsAllowed(LotusPermission.ShareHistory)
        : bridge.IsAllowed(LotusPermission.ListAndOpenGames);

    public Task<PortsListGamesResult> ListGamesAsync() => OnUi(() =>
    {
        var list = new List<PortsGame>();
        foreach (var game in catalog.Library.Games)
        {
            var installedPort = game.AllPorts.Select(p => p.Port).FirstOrDefault(p => play.IsPortInstalled(p.Id));
            var favorite = catalog.StateOf(game.Id).Favorite;
            if (installedPort is null && !favorite) continue;
            var state = installedPort is null ? null : play.StateOf(installedPort);
            var gameState = state?.Installed is null ? PortsGameState.NotInstalled
                : state.Installed.LastLaunchFailed ? PortsGameState.Problem
                : state.UpdateAvailable ? PortsGameState.UpdateAvailable
                : PortsGameState.Ready;
            list.Add(new PortsGame(game.Id, game.Game.Title, game.Game.LotusGameKey, game.Game.OriginalPlatform.ToString(), gameState,
                state?.Installed?.ActiveTag, game.Game.Cover?.Url, favorite, state?.Installed?.LastRun?.At));
        }
        return new PortsListGamesResult(list);
    });

    public Task<PortsLaunchResult?> LaunchAsync(string gameId) => OnUi<PortsLaunchResult?>(() =>
    {
        if (catalog.Library.Find(gameId) is not { } game) return null;
        var port = game.AllPorts.Select(p => p.Port).FirstOrDefault(p => play.IsPortInstalled(p.Id));
        if (port is not null && play.StateOf(port) is { Installed.LastLaunchFailed: false, Progress: null, RunningSince: null } && play.Launch(port) is null)
            return new PortsLaunchResult(true, null);
        // Not installed, busy, or with a problem: the player decides on the game page.
        ShowGame(game.Id);
        return new PortsLaunchResult(false, Strings.Get("LotusLaunchOpenedPage"));
    });

    public Task<bool> OpenAsync(string? gameId) => OnUi(() =>
    {
        if (gameId is not null && catalog.Library.Find(gameId) is null) return false;
        if (gameId is null) Front(() => App.MainWindow?.Navigate(Pages.Library));
        else ShowGame(gameId);
        return true;
    });

    public async Task<JournalQueryResult> JournalAsync(int limit)
    {
        var entries = await journal.ListRecentAsync(limit);
        return new JournalQueryResult(entries.Select(e => new JournalEntryDto(e.Id, e.StartedAt, e.FinishedAt, e.Kind.ToString(), e.PortId, e.Outcome?.ToString(),
            e.Details, e.CorrelationId.ToString("D"))).ToList());
    }

    public void OnEvent(JsonRpcMessage message) => bridge.OnLotusEvent(message);

    private void ShowGame(string gameId) => Front(() => App.MainWindow?.Navigate(Pages.Game, gameId));

    /// <summary>Brings the window back (restored and in front) and runs the navigation.</summary>
    private static void Front(Action navigate)
    {
        if (App.MainWindow is not { } window) return;
        if (window.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized } presenter) presenter.Restore();
        navigate();
        window.Activate();
    }

    private Task<T> OnUi<T>(Func<T> work)
    {
        if (ui is null || ui.HasThreadAccess) return Task.FromResult(work());
        var done = new TaskCompletionSource<T>();
        if (!ui.TryEnqueue(() =>
        {
            try { done.SetResult(work()); }
            catch (Exception ex) { done.SetException(ex); }
        })) done.SetException(new InvalidOperationException("The window is closing."));
        return done.Task;
    }
}
