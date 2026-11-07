using System.Net;
using LotusPorts.Catalog;
using LotusPorts.Core.Guides;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Catalog;

/// <summary>
/// The port's "how to play" guide and where it came from (the README on GitHub). <see cref="Translated"/>: the catalog's
/// translation is shown; <see cref="Outdated"/>: the authors changed the original after it was translated.
/// </summary>
public sealed record PortGuideResult(PortGuide Guide, Uri Source, bool Translated = false, bool Outdated = false);

/// <summary>
/// Reads the README of the port's repository from raw.githubusercontent.com (not limited like the API) and keeps a copy
/// in cache\guides for a day, also used when offline. In Portuguese, the authors' own Portuguese section is preferred;
/// otherwise the catalog's translation of their English section, otherwise the English as they wrote it.
/// </summary>
public sealed class PortGuideService(AppPaths paths, GitHubClient github, TimeProvider time, ILogger<PortGuideService> logger)
{
    private static readonly string[] Names = ["README.md", "readme.md", "Readme.md", "README.MD"];
    private static readonly TimeSpan FreshFor = TimeSpan.FromDays(1);

    private string Folder => Path.Combine(paths.Root, "cache", "guides");

    public async Task<PortGuideResult?> GetAsync(Port port, string language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var (markdown, name) = await ReadmeAsync(port, cancellationToken).ConfigureAwait(false);
        var baseUrl = new Uri($"https://github.com/{port.Repo.Owner}/{port.Repo.Name}/blob/HEAD/");
        var original = markdown is null ? null : ReadmeGuide.Extract(markdown, baseUrl, language);
        var source = name is null ? null : new Uri($"https://github.com/{port.Repo.Owner}/{port.Repo.Name}/blob/HEAD/{name}");

        var wantPortuguese = language.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
        if (wantPortuguese && original is not { Portuguese: true } && GuideTranslations.Find(port.Id, language) is { } translation)
        {
            var outdated = original is not null && ReadmeGuide.Fingerprint(original) != translation.Fingerprint;
            return new PortGuideResult(ReadmeGuide.FromMarkdown(translation.Markdown, baseUrl, portuguese: true), source ?? translation.Source, Translated: true, outdated);
        }
        return original is null || source is null ? null : new PortGuideResult(original, source);
    }

    /// <summary>Screenshots the port shows in its own README (addresses only; loaded when shown).</summary>
    public async Task<IReadOnlyList<Uri>> ScreenshotsAsync(Port port, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var (markdown, _) = await ReadmeAsync(port, cancellationToken).ConfigureAwait(false);
        if (markdown is null) return [];
        var rawBase = new Uri($"https://raw.githubusercontent.com/{Uri.EscapeDataString(port.Repo.Owner)}/{Uri.EscapeDataString(port.Repo.Name)}/HEAD/");
        return ReadmeImages.Extract(markdown, rawBase);
    }

    private async Task<(string? Markdown, string? Name)> ReadmeAsync(Port port, CancellationToken cancellationToken)
    {
        var key = $"{port.Repo.Owner}__{port.Repo.Name}".ToLowerInvariant();
        foreach (var c in Path.GetInvalidFileNameChars()) key = key.Replace(c, '_');
        var cacheFile = Path.Combine(Folder, key + ".md");
        var nameFile = Path.Combine(Folder, key + ".name");

        string? name = File.Exists(nameFile) ? (await File.ReadAllTextAsync(nameFile, cancellationToken).ConfigureAwait(false)).Trim() : null;
        if (name is not null && File.Exists(cacheFile) && time.GetUtcNow() - File.GetLastWriteTimeUtc(cacheFile) < FreshFor)
            return (await File.ReadAllTextAsync(cacheFile, cancellationToken).ConfigureAwait(false), name);

        try
        {
            foreach (var candidate in name is null ? Names : [name, .. Names.Where(n => n != name)])
            {
                var url = new Uri($"https://raw.githubusercontent.com/{Uri.EscapeDataString(port.Repo.Owner)}/{Uri.EscapeDataString(port.Repo.Name)}/HEAD/{candidate}");
                var result = await github.GetTextAsync(url, null, cancellationToken).ConfigureAwait(false);
                if (result.Status == HttpStatusCode.NotFound) continue;
                if (result.Body is null) break;
                Directory.CreateDirectory(Folder);
                await File.WriteAllTextAsync(cacheFile, result.Body, cancellationToken).ConfigureAwait(false);
                await File.WriteAllTextAsync(nameFile, candidate, cancellationToken).ConfigureAwait(false);
                return (result.Body, candidate);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException { CancellationToken.IsCancellationRequested: false } or InvalidDataException)
        {
            logger.LogInformation(ex, "README not available for {Port}", port.Id);
        }
        // Offline or GitHub unavailable: the last copy is better than nothing.
        return name is not null && File.Exists(cacheFile)
            ? (await File.ReadAllTextAsync(cacheFile, cancellationToken).ConfigureAwait(false), name)
            : (null, null);
    }
}
