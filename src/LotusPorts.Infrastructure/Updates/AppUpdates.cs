using System.Diagnostics;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Updates;

/// <summary>A newer Lotus Ports installer published on GitHub, with the SHA-256 GitHub recorded for it.</summary>
public sealed record AppRelease(string Tag, Version Version, bool Prerelease, ReleaseAssetInfo Installer, DateTimeOffset? PublishedAt);

public enum AppUpdateState
{
    /// <summary>No repository is set yet: Lotus Ports has not been published.</summary>
    NotPublished,
    UpToDate,
    Available,
    Offline,
    RateLimited,
}

public sealed record AppUpdateCheck(AppUpdateState State, AppRelease? Release);

/// <summary>Picks the newest installer newer than the running version. Pure, so it can be tested.</summary>
public static class AppUpdatePicker
{
    /// <summary>"v1.2.3", "1.2.3-alpha", "v0.2.0-beta.1" → 1.2.3 / 0.2.0; anything else is not a version.</summary>
    public static (Version Version, bool Prerelease)? Parse(string tag)
    {
        var t = tag.Trim().TrimStart('v', 'V');
        var dash = t.IndexOfAny(['-', '+']);
        var core = dash < 0 ? t : t[..dash];
        if (!Version.TryParse(core, out var v)) return null;
        return (new Version(v.Major, v.Minor, Math.Max(v.Build, 0)), dash >= 0 && t[dash] == '-');
    }

    /// <summary>
    /// The newest release whose version is greater than <paramref name="current"/> and that publishes an installer
    /// ("*Setup*.exe") with a SHA-256. Test versions are offered only to people already on a test version.
    /// The same version as a test build (0.2.0-alpha) is not newer than the final 0.2.0.
    /// </summary>
    public static AppRelease? Pick(IReadOnlyList<ReleaseInfo> releases, string current)
    {
        ArgumentNullException.ThrowIfNull(releases);
        if (Parse(current) is not { } now) return null;
        AppRelease? best = null;
        foreach (var r in releases)
        {
            if (Parse(r.Tag) is not { } v) continue;
            var pre = r.Prerelease || v.Prerelease;
            if (pre && !now.Prerelease) continue;
            var newer = v.Version > now.Version || (v.Version == now.Version && now.Prerelease && !pre);
            if (!newer || (best is not null && v.Version <= best.Version)) continue;
            var installer = r.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                && a.Name.Contains("Setup", StringComparison.OrdinalIgnoreCase) && a.Sha256 is { Length: 64 });
            if (installer is null) continue;
            best = new AppRelease(r.Tag, v.Version, pre, installer, r.PublishedAt);
        }
        return best;
    }
}

/// <summary>
/// Lotus Ports updating itself from the releases of its own repository. Checking only reads GitHub; the installer is
/// downloaded only after the player confirms, its SHA-256 must match what GitHub recorded, and it then runs with the
/// usual Windows prompts. Nothing happens while <see cref="Repository"/> is unset (before the first publication).
/// </summary>
public sealed class AppUpdateService(GitHubClient github, AppPaths paths, ILogger<AppUpdateService> logger)
{
    /// <summary>Owner/name of Lotus Ports' own GitHub repository. Set when the app is published.</summary>
    public static readonly (string Owner, string Name)? Repository = null;

    public async Task<AppUpdateCheck> CheckAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        if (Repository is not { } repo) return new(AppUpdateState.NotPublished, null);
        try
        {
            var releases = await github.GetReleasesAsync(repo.Owner, repo.Name, 15, cancellationToken).ConfigureAwait(false);
            return AppUpdatePicker.Pick(releases, currentVersion) is { } r ? new(AppUpdateState.Available, r) : new(AppUpdateState.UpToDate, null);
        }
        catch (GitHubRateLimitException)
        {
            return new(AppUpdateState.RateLimited, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException { CancellationToken.IsCancellationRequested: false })
        {
            logger.LogInformation(ex, "Could not check for Lotus Ports updates");
            return new(AppUpdateState.Offline, null);
        }
    }

    /// <summary>Downloads the installer and checks its SHA-256. Returns the file, or throws when it does not match.</summary>
    public async Task<string> DownloadAsync(AppRelease release, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(release);
        var folder = Path.Combine(paths.Data, "app-update");
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, Path.GetFileName(release.Installer.Name));
        var part = file + ".part";
        try
        {
            var sha = await github.DownloadAsync(release.Installer, part, progress, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(sha, release.Installer.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The installer does not match the SHA-256 recorded by GitHub.");
            File.Move(part, file, overwrite: true);
            return file;
        }
        finally
        {
            if (File.Exists(part)) File.Delete(part);
        }
    }

    /// <summary>
    /// Starts the verified installer with its own progress window and no questions (it was confirmed in the app);
    /// /relaunch=1 makes it open Lotus Ports again. The caller closes Lotus Ports right after, so files can be replaced.
    /// </summary>
    public static void RunInstaller(string file) =>
        Process.Start(new ProcessStartInfo(file, "/SILENT /SUPPRESSMSGBOXES /NORESTART /relaunch=1") { UseShellExecute = true });
}
