using System.Diagnostics;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Install;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace LotusPorts.App;

/// <summary>What the release lookup found for a port: the version to install, or why there is none.</summary>
public sealed record ReleaseLookup(ReleasePick? Pick, DateTimeOffset? RateLimitedUntil, bool Offline, DateTimeOffset CheckedAt, bool FromCatalog = false)
{
    public ReleaseChoice? Choice => Pick?.Choice;
}

/// <summary>Everything the game page needs to draw the main button for one port.</summary>
public sealed record PortRuntime(
    InstalledPort? Installed,
    ReleaseLookup? Latest,
    InstallProgress? Progress,
    DateTimeOffset? RunningSince,
    string? Problem)
{
    /// <summary>The newest suitable version differs from the active one (and the version is not pinned).</summary>
    public bool UpdateAvailable => Installed is { PinnedTag: null } i && Latest?.Choice is { } c && c.Release.Tag != i.ActiveTag
        && c.Release.Tag != i.PreviousTag;
}

/// <summary>
/// The "Jogar" orchestrator for this phase: looks up releases, installs/updates/rolls back/removes through
/// <see cref="PortInstaller"/> and opens the game. One operation per port at a time; state changes raise
/// <see cref="Changed"/> on the UI thread.
/// </summary>
public sealed class PlayService(PortLibrary library, PortInstaller installer, GameLauncher launcher, GitHubClient github,
    IOperationJournal journal, PreferencesService preferences, TimeProvider time, ILogger<PlayService> logger, LotusBridge lotus, LotusPorts.Infrastructure.Catalog.CatalogService catalog)
{
    private static readonly TimeSpan LookupFreshFor = TimeSpan.FromMinutes(30);

    private readonly Dictionary<string, ReleaseLookup> lookups = [];
    private readonly Dictionary<string, (InstallProgress Progress, CancellationTokenSource Cancel)> working = [];
    private readonly Dictionary<string, Port> workingPorts = [];

    // One download at a time: the others wait "Na fila" and each finishes sooner than if they shared the connection.
    private readonly SemaphoreSlim downloadQueue = new(1, 1);

    /// <summary>Installs and updates in progress, for the Downloads screen.</summary>
    public IReadOnlyList<(Port Port, InstallProgress Progress)> Working =>
        working.Where(w => workingPorts.ContainsKey(w.Key)).Select(w => (workingPorts[w.Key], w.Value.Progress)).ToList();
    private readonly Dictionary<string, (Process Process, DateTimeOffset Since)> running = [];
    private readonly Dictionary<string, string> problems = [];
    private DispatcherQueue? ui;

    /// <summary>The port id whose state changed.</summary>
    public event Action<string>? Changed;

    /// <summary>
    /// At startup: the library folder from Configurações, leftovers of installs interrupted by closing the app, and
    /// journal entries left open by them (recorded as cancelled, nothing is guessed about their result).
    /// </summary>
    public async Task InitializeAsync(DispatcherQueue dispatcher)
    {
        ui = dispatcher;
        library.UseFolder((await preferences.LoadAsync().ConfigureAwait(false)).LibraryFolder);
        RefreshInstalled();
        try
        {
            var cleaned = library.CleanLeftovers();
            foreach (var open in await journal.ListOpenAsync().ConfigureAwait(false))
                await journal.CompleteAsync(open.Id, OperationOutcome.Cancelled, null, "Interrupted: the app closed before it finished.").ConfigureAwait(false);
            if (cleaned > 0) logger.LogInformation("Removed {Count} leftovers of interrupted installs", cleaned);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Could not clean interrupted installs");
        }
    }

    public PortRuntime StateOf(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        return new PortRuntime(
            library.Read(port.Id),
            lookups.GetValueOrDefault(port.Id),
            working.TryGetValue(port.Id, out var w) ? w.Progress : null,
            running.TryGetValue(port.Id, out var r) ? r.Since : null,
            problems.GetValueOrDefault(port.Id));
    }

    public bool IsInstalled(string portId) => library.Read(portId) is not null;

    /// <summary>When this port was last opened, from its active.json (null if never or not installed).</summary>
    public DateTimeOffset? LastPlayed(string portId) => installedPorts.Contains(portId) ? library.Read(portId)?.LastRun?.At : null;

    public string Folder(Port port) => library.PortFolder(port.Id);

    private DispatcherQueueTimer? dailyCheck;

    /// <summary>Installed ports with a newer version found by the last check (not pinned, not busy).</summary>
    public IReadOnlyList<Port> UpdatesAvailable =>
        InstalledCatalogPorts().Where(p => !working.ContainsKey(p.Id) && !running.ContainsKey(p.Id) && StateOf(p).UpdateAvailable).ToList();

    private IEnumerable<Port> InstalledCatalogPorts() =>
        catalog.Library.Games.SelectMany(g => g.AllPorts.Select(p => p.Port)).Where(p => installedPorts.Contains(p.Id)).DistinctBy(p => p.Id);

    /// <summary>
    /// Looks for new versions of installed ports: a little after startup and then once a day while the app is open.
    /// Only reads GitHub (one request per installed port); nothing is downloaded until the player confirms.
    /// </summary>
    public void StartDailyUpdateCheck()
    {
        if (ui is null || dailyCheck is not null) return;
        dailyCheck = ui.CreateTimer();
        dailyCheck.Interval = TimeSpan.FromSeconds(20);
        dailyCheck.Tick += async (t, _) =>
        {
            t.Interval = TimeSpan.FromHours(24);
            await CheckUpdatesAsync();
            // "Atualizações automáticas" (on by default) is the player's standing consent: new versions install by
            // themselves, one at a time, never while the game is open. The previous version is kept for "Voltar".
            if ((await preferences.LoadAsync()).AutoUpdate && UpdatesAvailable is { Count: > 0 } updates)
            {
                logger.LogInformation("Updating {Count} ports automatically", updates.Count);
                await UpdateAllAsync(updates);
            }
        };
        dailyCheck.Start();
    }

    public async Task CheckUpdatesAsync()
    {
        foreach (var port in InstalledCatalogPorts().ToList())
        {
            var lookup = await LookupAsync(port);
            if (lookup.RateLimitedUntil is not null) break; // the hourly limit: try again on the next check
        }
    }

    /// <summary>Updates the given ports one after another. The caller already asked the player.</summary>
    public async Task<int> UpdateAllAsync(IReadOnlyList<Port> ports)
    {
        var failed = 0;
        foreach (var port in ports)
        {
            if (StateOf(port) is not { UpdateAvailable: true, Latest.Choice: { } choice }) continue;
            if (await InstallAsync(port, choice) is not null || problems.ContainsKey(port.Id)) failed++;
        }
        return failed;
    }

    /// <summary>Asks GitHub for the port's releases (cached for a while). Rate limits and network errors are reported, not thrown.</summary>
    public async Task<ReleaseLookup> LookupAsync(Port port, bool force = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (!force && lookups.TryGetValue(port.Id, out var cached) && time.GetUtcNow() - cached.CheckedAt < LookupFreshFor) return cached;
        ReleaseLookup lookup;
        try
        {
            var releases = await github.GetReleasesAsync(port.Repo.Owner, port.Repo.Name, 15, cancellationToken);
            var pinned = library.Read(port.Id)?.PinnedTag;
            lookup = new ReleaseLookup(ReleasePicker.Pick(releases, port.WindowsAsset, allowPrerelease: false, pinnedTag: pinned), null, false, time.GetUtcNow());
        }
        catch (GitHubRateLimitException ex)
        {
            // GitHub's hourly limit only stops the lookup: the version the catalog checked can still be downloaded
            // (download links are not limited) and is verified against the SHA-256 the catalog recorded.
            var fromCatalog = FromCatalog(port);
            lookup = new ReleaseLookup(fromCatalog is null ? null : new ReleasePick(fromCatalog, ReleaseProblem.None, [fromCatalog.Asset]),
                ex.ResetAt ?? time.GetUtcNow().AddHours(1), false, time.GetUtcNow(), FromCatalog: fromCatalog is not null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException { CancellationToken.IsCancellationRequested: false })
        {
            logger.LogInformation(ex, "Releases not available for {Port}", port.Id);
            lookup = new ReleaseLookup(null, null, true, time.GetUtcNow());
        }
        lookups[port.Id] = lookup;
        Raise(port.Id);
        return lookup;
    }

    /// <summary>Installs or updates to <paramref name="choice"/>. The caller already asked the player.</summary>
    public async Task<InstallFailure?> InstallAsync(Port port, ReleaseChoice choice)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (working.ContainsKey(port.Id) || running.ContainsKey(port.Id)) return null;
        using var cancel = new CancellationTokenSource();
        working[port.Id] = (new InstallProgress(InstallStage.Queued, 0), cancel);
        workingPorts[port.Id] = port;
        problems.Remove(port.Id);
        Raise(port.Id);
        var queued = false;
        var lastReport = DateTimeOffset.MinValue;
        var progress = new Progress<InstallProgress>(p =>
        {
            // The bar moves smoothly enough at ~10 updates a second; more would only cost CPU.
            var now = time.GetUtcNow();
            if (p.Stage == working[port.Id].Progress.Stage && p.Fraction < 1 && now - lastReport < TimeSpan.FromMilliseconds(100)) return;
            lastReport = now;
            if (working.ContainsKey(port.Id)) working[port.Id] = (p, cancel);
            Raise(port.Id);
        });
        try
        {
            await downloadQueue.WaitAsync(cancel.Token);
            queued = true;
            working[port.Id] = (new InstallProgress(InstallStage.Downloading, 0), cancel);
            Raise(port.Id);
            var existed = library.Read(port.Id) is not null;
            var done = await installer.InstallAsync(port, choice, progress, cancel.Token);
            _ = lotus.NotifyAsync(existed ? Lotus.Integration.Contracts.Events.GameUpdated : Lotus.Integration.Contracts.Events.GameInstalled, new(port.GameId, port.Id, done.ActiveTag));
            return null;
        }
        catch (OperationCanceledException)
        {
            problems[port.Id] = Strings.Get("PlayCancelled");
            return null;
        }
        catch (InstallException ex)
        {
            problems[port.Id] = Strings.Get(ex.Failure switch
            {
                InstallFailure.IntegrityMismatch => "PlayFailedIntegrity",
                InstallFailure.NoExecutable => "PlayFailedProgram",
                _ => "PlayFailed",
            });
            return ex.Failure;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException or InvalidDataException or GitHubRateLimitException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Install of {Port} failed", port.Id);
            problems[port.Id] = Strings.Get(ex is ExtractionException { Problem: ExtractionProblem.TooLarge } ? "PlayFailedTooLarge"
                : ex is IOException { HResult: unchecked((int)0x80070070) } ? "PlayFailedDiskFull"
                : ex is HttpRequestException ? "PlayFailedNetwork" : "PlayFailed");
            return InstallFailure.NotInstalled;
        }
        finally
        {
            if (queued) downloadQueue.Release();
            working.Remove(port.Id);
            workingPorts.Remove(port.Id);
            RefreshInstalled();
            Raise(port.Id);
        }
    }

    /// <summary>The release the catalog verified (tag, file, size and SHA-256 all recorded), as a download choice.</summary>
    public static ReleaseChoice? FromCatalog(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (port.Evidence is not { ReleaseTag: { } tag, AssetName: { } name, AssetSize: > 0 and var size, AssetSha256: { Length: 64 } sha, Prerelease: false } evidence)
            return null;
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, port.WindowsAsset.Pattern, System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(1))) return null;
        var url = new Uri($"https://github.com/{Uri.EscapeDataString(port.Repo.Owner)}/{Uri.EscapeDataString(port.Repo.Name)}/releases/download/{Uri.EscapeDataString(tag)}/{Uri.EscapeDataString(name)}");
        var asset = new ReleaseAssetInfo(name, size, sha, url);
        DateTimeOffset? published = DateTimeOffset.TryParse(evidence.ReleaseDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var d) ? d : null;
        return new ReleaseChoice(new ReleaseInfo(tag, tag, false, published, [asset]), asset);
    }

    public void Cancel(string portId)
    {
        if (working.TryGetValue(portId, out var w)) w.Cancel.Cancel();
    }

    public async Task RollbackAsync(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (working.ContainsKey(port.Id) || running.ContainsKey(port.Id)) return;
        try
        {
            var back = await installer.RollbackAsync(port);
            _ = lotus.NotifyAsync(Lotus.Integration.Contracts.Events.GameRolledBack, new(port.GameId, port.Id, back.ActiveTag));
            problems.Remove(port.Id);
        }
        catch (Exception ex) when (ex is InstallException or IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Rollback of {Port} failed", port.Id);
            problems[port.Id] = Strings.Get("PlayRollbackFailed");
        }
        Raise(port.Id);
    }

    public async Task RemoveAsync(Port port, bool keepSaves)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (working.ContainsKey(port.Id) || running.ContainsKey(port.Id)) return;
        try
        {
            await Task.Run(() => installer.RemoveAsync(port, keepSaves));
            _ = lotus.NotifyAsync(Lotus.Integration.Contracts.Events.GameRemoved, new(port.GameId, port.Id, null));
            problems.Remove(port.Id);
        }
        catch (Exception ex) when (ex is InstallException or IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Remove of {Port} failed", port.Id);
            problems[port.Id] = Strings.Get("PlayRemoveFailed");
        }
        RefreshInstalled();
        Raise(port.Id);
    }

    /// <summary>"Manter esta versão": the player looked at the problem and keeps the current version.</summary>
    public void KeepVersion(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        installer.ClearProblem(port.Id);
        problems.Remove(port.Id);
        Raise(port.Id);
    }

    /// <summary>Opens the active version. A quick crash right after an update turns the button into "Resolver".</summary>
    public string? Launch(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (running.ContainsKey(port.Id) || library.Read(port.Id) is not { } manifest) return null;
        var result = launcher.Start(port, manifest);
        if (result.Process is not { } process)
        {
            var message = Strings.Get(result.Problem switch
            {
                LaunchProblem.NeedsAdministrator => "PlayNeedsAdmin",
                LaunchProblem.MissingProgram => "PlayMissingProgram",
                _ => "PlayCouldNotStart",
            });
            problems[port.Id] = message;
            Raise(port.Id);
            return message;
        }
        var since = time.GetUtcNow();
        running[port.Id] = (process, since);
        problems.Remove(port.Id);
        // With Lotus: a session (Lotus applies the game's performance profile and measures it). Never delays the game.
        var game = catalog.Library.Find(port.GameId);
        var session = lotus.SessionStartAsync(new Lotus.Integration.Contracts.SessionStartParams(
            game?.Game.LotusGameKey ?? "port:" + port.GameId, game?.Game.Title ?? port.Title, port.Id, manifest.ActiveTag,
            launcher.ProgramPath(port, manifest), process.Id, manifest.Style?.Name));
        process.Exited += (_, _) =>
        {
            int code;
            try { code = process.ExitCode; } catch (InvalidOperationException) { code = 0; }
            var ranFor = time.GetUtcNow() - since;
            process.Dispose();
            _ = EndSessionAsync(session, port, code, ranFor);
            ui?.TryEnqueue(() =>
            {
                running.Remove(port.Id);
                var after = installer.RecordLaunch(port.Id, code, ranFor);
                if (after is { LastLaunchFailed: true, LastRun: { } run }) problems[port.Id] = Strings.Get(run.FirstAfterUpdate ? "PlayFailedAfterUpdate" : "PlayFailedQuickly");
                Raise(port.Id);
            });
        };
        Raise(port.Id);
        return null;
    }

    /// <summary>Ends the Lotus session and remembers it for "Sua experiência" (report and suggestions).</summary>
    private async Task EndSessionAsync(Task<string?> session, Port port, int exitCode, TimeSpan ranFor)
    {
        if (await session.ConfigureAwait(false) is not { } id) return;
        await lotus.SessionEndAsync(id, exitCode, Math.Round(ranFor.TotalSeconds, 1)).ConfigureAwait(false);
        await lotus.RememberSessionAsync(port.GameId, id).ConfigureAwait(false);
        Raise(port.Id);
    }


    public void CloseGame(string portId)
    {
        if (!running.TryGetValue(portId, out var r)) return;
        try { r.Process.CloseMainWindow(); }
        catch (InvalidOperationException) { }
    }

    private HashSet<string> installedPorts = [];

    /// <summary>Installed ports, kept in memory so shelves can ask for hundreds of games cheaply.</summary>
    public bool IsPortInstalled(string portId) => installedPorts.Contains(portId);

    private void RefreshInstalled() => installedPorts = [.. library.ReadAll().Keys];

    private void Raise(string portId)
    {
        if (ui is null || ui.HasThreadAccess) Changed?.Invoke(portId);
        else ui.TryEnqueue(() => Changed?.Invoke(portId));
    }
}
