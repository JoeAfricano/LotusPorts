using System.Net;
using LotusPorts.Catalog;
using LotusPorts.Core.Guides;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Catalog;

/// <summary>The port's "how to play" guide and where it came from (the README on GitHub).</summary>
public sealed record PortGuideResult(PortGuide Guide, Uri Source);

/// <summary>
/// Reads the README of the port's repository from raw.githubusercontent.com (not limited like the API) and keeps a copy
/// in cache\guides for a day, also used when offline. The text is shown as the authors wrote it, with a link to them;
/// nothing is copied into the catalog.
/// </summary>
public sealed class PortGuideService(AppPaths paths, GitHubClient github, TimeProvider time, ILogger<PortGuideService> logger)
{
    private static readonly string[] Names = ["README.md", "readme.md", "Readme.md", "README.MD"];
    private static readonly TimeSpan FreshFor = TimeSpan.FromDays(1);

    private string Folder => Path.Combine(paths.Root, "cache", "guides");

    public async Task<PortGuideResult?> GetAsync(Port port, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var key = $"{port.Repo.Owner}__{port.Repo.Name}".ToLowerInvariant();
        foreach (var c in Path.GetInvalidFileNameChars()) key = key.Replace(c, '_');
        var cacheFile = Path.Combine(Folder, key + ".md");
        var nameFile = Path.Combine(Folder, key + ".name");

        string? markdown = null;
        string? name = File.Exists(nameFile) ? (await File.ReadAllTextAsync(nameFile, cancellationToken).ConfigureAwait(false)).Trim() : null;
        var fresh = File.Exists(cacheFile) && time.GetUtcNow() - File.GetLastWriteTimeUtc(cacheFile) < FreshFor;
        if (fresh && name is not null) markdown = await File.ReadAllTextAsync(cacheFile, cancellationToken).ConfigureAwait(false);

        if (markdown is null)
        {
            try
            {
                foreach (var candidate in name is null ? Names : [name, .. Names.Where(n => n != name)])
                {
                    var url = new Uri($"https://raw.githubusercontent.com/{Uri.EscapeDataString(port.Repo.Owner)}/{Uri.EscapeDataString(port.Repo.Name)}/HEAD/{candidate}");
                    var result = await github.GetTextAsync(url, null, cancellationToken).ConfigureAwait(false);
                    if (result.Status == HttpStatusCode.NotFound) continue;
                    if (result.Body is null) break;
                    markdown = result.Body;
                    name = candidate;
                    Directory.CreateDirectory(Folder);
                    await File.WriteAllTextAsync(cacheFile, markdown, cancellationToken).ConfigureAwait(false);
                    await File.WriteAllTextAsync(nameFile, name, cancellationToken).ConfigureAwait(false);
                    break;
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException { CancellationToken.IsCancellationRequested: false } or InvalidDataException)
            {
                logger.LogInformation(ex, "README not available for {Port}", port.Id);
            }
            // Offline or GitHub unavailable: the last copy is better than nothing.
            if (markdown is null && File.Exists(cacheFile) && name is not null)
                markdown = await File.ReadAllTextAsync(cacheFile, cancellationToken).ConfigureAwait(false);
        }
        if (markdown is null || name is null) return null;

        var source = new Uri($"https://github.com/{port.Repo.Owner}/{port.Repo.Name}/blob/HEAD/{name}");
        var guide = ReadmeGuide.Extract(markdown, new Uri($"https://github.com/{port.Repo.Owner}/{port.Repo.Name}/blob/HEAD/"));
        return guide is null ? null : new PortGuideResult(guide, source);
    }
}
