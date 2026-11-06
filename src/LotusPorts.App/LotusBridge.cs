using Lotus.Integration.Contracts;
using LotusPorts.Core.Diagnostics;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Verification;
using LotusPorts.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace LotusPorts.App;

/// <summary>The integration switches (master prompt, section 12.10). All on by default; each one is a promise kept here.</summary>
public enum LotusPermission
{
    Dependencies,
    Diagnostics,
    PerformanceProfile,
    Reports,
    ListAndOpenGames,
    ShareHistory,
}

/// <summary>
/// Lotus Ports' side of the family: keeps the connection to Lotus in the background, answers what the interface may use
/// (connected + Lotus announced it + the player allowed it) and relays game events. Without Lotus everything still
/// works; these calls just do nothing.
/// </summary>
public sealed class LotusBridge(LotusClient client, ISettingsStore settings, ILogger<LotusBridge> logger) : IAsyncDisposable
{
    private readonly CancellationTokenSource stop = new();
    private readonly Dictionary<LotusPermission, bool> permissions = Enum.GetValues<LotusPermission>().ToDictionary(p => p, _ => true);
    private DispatcherQueue? ui;

    public event Action? Changed;

    public LotusStatus Status => client.Status;

    public bool IsAllowed(LotusPermission permission) => permissions[permission];

    /// <summary>Connected, announced by Lotus and allowed by the player.</summary>
    public bool CanUse(string capability, LotusPermission permission) => client.Supports(capability) && IsAllowed(permission);

    public async Task StartAsync(DispatcherQueue dispatcher)
    {
        ui = dispatcher;
        foreach (var permission in Enum.GetValues<LotusPermission>())
            if (bool.TryParse(await settings.GetAsync(Key(permission)), out var allowed)) permissions[permission] = allowed;
        client.StatusChanged += _ => Raise();
        // Checks every 30 s: a Lotus opened later is found without restarting; a closed one is retried quietly.
        _ = Task.Run(() => client.RunAsync(TimeSpan.FromSeconds(30), stop.Token));
    }

    public async Task SetAllowedAsync(LotusPermission permission, bool allowed)
    {
        permissions[permission] = allowed;
        await settings.SetAsync(Key(permission), allowed.ToString());
        Raise();
    }

    public async Task RetryAsync()
    {
        await client.ConnectAsync();
        Raise();
    }

    /// <summary>Lotus's own system checks for "Resolver", as extra lines. Empty when unavailable or not allowed.</summary>
    public async Task<IReadOnlyList<ResolveCheck>> SystemChecksAsync(IReadOnlyList<string> graphicsApis, string? exePath, bool lastRunFailed)
    {
        var extra = new List<ResolveCheck>();
        if (!IsAllowed(LotusPermission.Diagnostics)) return extra;
        try
        {
            if (client.Supports(Methods.DiagnosticsRunSystemChecks)
                && await client.RequestAsync<SystemChecksParams, SystemChecksResult>(Methods.DiagnosticsRunSystemChecks, new(graphicsApis, exePath)) is { } result)
                extra.AddRange(result.Items.Select(i => new ResolveCheck(ResolveCheckKind.Graphics, Map(i.State), "FromLotus", [i.TitlePtBr, i.DetailsPtBr])));
            if (lastRunFailed && client.Supports(Methods.JournalQueryRecent)
                && await client.RequestAsync<JournalQueryRecentParams, JournalQueryRecentResult>(Methods.JournalQueryRecent, new(7)) is { Changes.Count: > 0 } changes)
            {
                var latest = changes.Changes.OrderByDescending(c => c.At).First();
                extra.Add(new ResolveCheck(ResolveCheckKind.RecentUpdate, CheckState.Warning, "SystemChangedBefore",
                    [latest.DescriptionPtBr, latest.At.ToLocalTime().ToString("g", System.Globalization.CultureInfo.CurrentCulture)]));
            }
        }
        catch (Exception ex) when (ex is LotusUnavailableException or LotusRemoteException or NotSupportedException)
        {
            logger.LogInformation(ex, "Lotus checks not available");
        }
        return extra;
    }

    /// <summary>Tells Lotus a game started (it applies the game's performance profile). Returns the session id, or null.</summary>
    public async Task<string?> SessionStartAsync(SessionStartParams session)
    {
        if (!CanUse(Methods.SessionStart, LotusPermission.PerformanceProfile)) return null;
        try { return (await client.RequestAsync<SessionStartParams, SessionStartResult>(Methods.SessionStart, session))?.SessionId; }
        catch (Exception ex) when (ex is LotusUnavailableException or LotusRemoteException or NotSupportedException)
        {
            logger.LogInformation(ex, "Session.Start not available");
            return null;
        }
    }

    public async Task SessionEndAsync(string sessionId, int exitCode, double seconds)
    {
        if (!client.Supports(Methods.SessionEnd)) return;
        try { await client.RequestAsync<SessionEndParams, object>(Methods.SessionEnd, new(sessionId, exitCode, seconds)); }
        catch (Exception ex) when (ex is LotusUnavailableException or LotusRemoteException or NotSupportedException)
        {
            logger.LogInformation(ex, "Session.End not delivered");
        }
    }

    /// <summary>Install/update/rollback/remove/style, for Lotus's single timeline of the PC (only if allowed).</summary>
    public Task NotifyAsync(string eventName, GameEvent body) =>
        IsAllowed(LotusPermission.ShareHistory) ? client.NotifyAsync(eventName, body) : Task.CompletedTask;

    private static CheckState Map(CheckStatus status) => status switch
    {
        CheckStatus.Ok => CheckState.Ok,
        CheckStatus.Warning => CheckState.Warning,
        CheckStatus.Failure => CheckState.Failure,
        _ => CheckState.NotVerified,
    };

    private static string Key(LotusPermission permission) => "lotus.allow." + permission;

    private void Raise()
    {
        if (ui is null || ui.HasThreadAccess) Changed?.Invoke();
        else ui.TryEnqueue(() => Changed?.Invoke());
    }

    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync();
        await client.DisposeAsync();
        stop.Dispose();
    }
}
