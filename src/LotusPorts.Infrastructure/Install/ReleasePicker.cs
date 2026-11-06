using System.Text.RegularExpressions;
using LotusPorts.Catalog;
using LotusPorts.Infrastructure.Network;

namespace LotusPorts.Infrastructure.Install;

public enum ReleaseProblem
{
    None,

    /// <summary>No published version (or only pre-releases, which need advanced mode).</summary>
    NoRelease,

    /// <summary>The newest version has no file for Windows that matches the catalog.</summary>
    NoWindowsFile,

    /// <summary>More than one file matches: Lotus Ports does not guess.</summary>
    Ambiguous,

    /// <summary>The file is an installer or a format Lotus Ports does not open by itself.</summary>
    UnsupportedPackage,
}

public sealed record ReleaseChoice(ReleaseInfo Release, ReleaseAssetInfo Asset);

public sealed record ReleasePick(ReleaseChoice? Choice, ReleaseProblem Problem, IReadOnlyList<ReleaseAssetInfo> Candidates)
{
    public bool Ok => Choice is not null;
}

/// <summary>
/// Chooses the Windows file of the newest suitable release with the catalog's windowsAsset rules. Exactly one
/// match is required; zero or several are reported, never guessed (master prompt, section 6).
/// </summary>
public static class ReleasePicker
{
    /// <summary>Package types Lotus Ports installs by itself (zip archives and single portable programs).</summary>
    public static bool CanInstall(WindowsAsset asset) => asset.PackageType is "zip" or "portable-exe";

    public static ReleasePick Pick(IReadOnlyList<ReleaseInfo> releases, WindowsAsset rules, bool allowPrerelease = false, string? pinnedTag = null)
    {
        ArgumentNullException.ThrowIfNull(releases);
        ArgumentNullException.ThrowIfNull(rules);
        if (!CanInstall(rules)) return new(null, ReleaseProblem.UnsupportedPackage, []);

        var pattern = new Regex(rules.Pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        var exclude = rules.ExcludePattern is { Length: > 0 } e ? new Regex(e, RegexOptions.None, TimeSpan.FromSeconds(1)) : null;
        var prerelease = allowPrerelease || rules.PreferPrerelease;

        var eligible = releases
            .Where(r => pinnedTag is null ? prerelease || !r.Prerelease : r.Tag == pinnedTag)
            .OrderByDescending(r => r.PublishedAt ?? DateTimeOffset.MinValue)
            .ToList();
        if (eligible.Count == 0) return new(null, ReleaseProblem.NoRelease, []);

        // The newest release that has any Windows file decides; an empty build (assets still uploading) is skipped.
        foreach (var release in eligible)
        {
            var matches = release.Assets.Where(a => pattern.IsMatch(a.Name) && (exclude is null || !exclude.IsMatch(a.Name))).ToList();
            if (matches.Count == 0) continue;
            return matches.Count == 1
                ? new(new ReleaseChoice(release, matches[0]), ReleaseProblem.None, matches)
                : new(null, ReleaseProblem.Ambiguous, matches);
        }
        return new(null, ReleaseProblem.NoWindowsFile, []);
    }
}
