using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LotusPorts.Infrastructure.Network;

/// <summary>The GitHub API refused because the hourly limit was reached. ResetAt says when it comes back.</summary>
public sealed class GitHubRateLimitException(DateTimeOffset? resetAt) : Exception("GitHub API rate limit reached.")
{
    public DateTimeOffset? ResetAt { get; } = resetAt;
}

public sealed record FetchResult(HttpStatusCode Status, string? Body, string? ETag)
{
    public bool NotModified => Status == HttpStatusCode.NotModified;
}

public sealed record ReleaseAssetInfo(string Name, long Size, string? Sha256, Uri DownloadUrl);

public sealed record ReleaseInfo(string Tag, string? Name, bool Prerelease, DateTimeOffset? PublishedAt, IReadOnlyList<ReleaseAssetInfo> Assets);

public sealed record RepositoryInfo(string Owner, string Name, string? Description, string DefaultBranch, string? License, bool Archived);

/// <summary>
/// Public GitHub data only: repository info, releases, raw files and the index at the end of release zips.
/// Requests go only to api.github.com, raw.githubusercontent.com and GitHub's release download hosts.
/// </summary>
public sealed class GitHubClient(HttpClient http)
{
    public const long MaxCatalogBytes = 4 * 1024 * 1024;

    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "api.github.com", "raw.githubusercontent.com", "github.com", "objects.githubusercontent.com", "release-assets.githubusercontent.com",
    };

    public async Task<RepositoryInfo?> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        using var doc = await GetApiJsonAsync($"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}", cancellationToken).ConfigureAwait(false);
        if (doc is null) return null;
        var r = doc.RootElement;
        var full = r.GetProperty("full_name").GetString()!.Split('/');
        return new RepositoryInfo(full[0], full[1],
            r.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null,
            r.GetProperty("default_branch").GetString() ?? "main",
            r.TryGetProperty("license", out var l) && l.ValueKind == JsonValueKind.Object && l.TryGetProperty("spdx_id", out var s) && s.GetString() is { } spdx && spdx != "NOASSERTION" ? spdx : null,
            r.TryGetProperty("archived", out var a) && a.GetBoolean());
    }

    public async Task<IReadOnlyList<ReleaseInfo>> GetReleasesAsync(string owner, string repo, int count = 10, CancellationToken cancellationToken = default)
    {
        using var doc = await GetApiJsonAsync($"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/releases?per_page={count}", cancellationToken).ConfigureAwait(false);
        if (doc is null) return [];
        return doc.RootElement.EnumerateArray()
            .Where(r => !r.GetProperty("draft").GetBoolean())
            .Select(r => new ReleaseInfo(
                r.GetProperty("tag_name").GetString()!,
                r.TryGetProperty("name", out var n) ? n.GetString() : null,
                r.GetProperty("prerelease").GetBoolean(),
                r.TryGetProperty("published_at", out var p) && p.ValueKind == JsonValueKind.String ? DateTimeOffset.Parse(p.GetString()!, CultureInfo.InvariantCulture) : null,
                r.GetProperty("assets").EnumerateArray().Select(a => new ReleaseAssetInfo(
                    a.GetProperty("name").GetString()!,
                    a.GetProperty("size").GetInt64(),
                    a.TryGetProperty("digest", out var dg) && dg.GetString() is { } digest && digest.StartsWith("sha256:", StringComparison.Ordinal) ? digest[7..] : null,
                    new Uri(a.GetProperty("browser_download_url").GetString()!))).ToList()))
            .ToList();
    }

    /// <summary>Downloads a small text file (a catalog) with ETag support. Never more than MaxCatalogBytes.</summary>
    public async Task<FetchResult> GetTextAsync(Uri url, string? etag, CancellationToken cancellationToken = default)
    {
        EnsureAllowed(url);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(etag) && EntityTagHeaderValue.TryParse(etag, out var tag)) request.Headers.IfNoneMatch.Add(tag);
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotModified) return new(HttpStatusCode.NotModified, null, etag);
        if (!response.IsSuccessStatusCode) return new(response.StatusCode, null, null);
        if (response.Content.Headers.ContentLength > MaxCatalogBytes) throw new InvalidDataException("Catalog file is too large.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var limited = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (limited.Length + read > MaxCatalogBytes) throw new InvalidDataException("Catalog file is too large.");
            limited.Write(buffer, 0, read);
        }
        return new(response.StatusCode, Encoding.UTF8.GetString(limited.ToArray()), response.Headers.ETag?.ToString());
    }

    /// <summary>Lists the files inside a release zip by reading only its index at the end (range request). Nothing is saved.</summary>
    public async Task<IReadOnlyList<string>> ListZipEntriesAsync(ReleaseAssetInfo asset, CancellationToken cancellationToken = default)
    {
        var size = asset.Size;
        var tailLength = Math.Min(size, 65_557);
        var tail = await RangeAsync(asset.DownloadUrl, size - tailLength, size - 1, cancellationToken).ConfigureAwait(false);
        var eocd = -1;
        for (var i = tail.Length - 22; i >= 0; i--)
            if (BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(i)) == 0x06054b50) { eocd = i; break; }
        if (eocd < 0) throw new InvalidDataException("Zip index not found.");
        var cdSize = BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocd + 12));
        var cdOffset = BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocd + 16));
        if (cdOffset == 0xFFFFFFFF || cdSize == 0xFFFFFFFF || cdSize > 8 * 1024 * 1024) throw new InvalidDataException("Zip index not supported.");
        var start = size - tailLength;
        var cd = cdOffset >= start
            ? tail.AsSpan((int)(cdOffset - start), (int)cdSize).ToArray()
            : await RangeAsync(asset.DownloadUrl, cdOffset, cdOffset + cdSize - 1, cancellationToken).ConfigureAwait(false);
        var names = new List<string>();
        for (var p = 0; p + 46 <= cd.Length && BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(p)) == 0x02014b50;)
        {
            int nameLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 28));
            int extraLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 30));
            int commentLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 32));
            if (p + 46 + nameLen > cd.Length) break;
            names.Add(Encoding.UTF8.GetString(cd, p + 46, nameLen).Replace('\\', '/'));
            p += 46 + nameLen + extraLen + commentLen;
        }
        return names;
    }

    private async Task<byte[]> RangeAsync(Uri url, long from, long to, CancellationToken cancellationToken)
    {
        EnsureAllowed(url);
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new RangeHeaderValue(from, to);
        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode != HttpStatusCode.PartialContent) throw new InvalidDataException("The server did not accept a partial read.");
        EnsureAllowed(response.RequestMessage?.RequestUri ?? url);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<JsonDocument?> GetApiJsonAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        if ((response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            && response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining) && remaining.FirstOrDefault() == "0")
        {
            DateTimeOffset? reset = response.Headers.TryGetValues("X-RateLimit-Reset", out var r) && long.TryParse(r.FirstOrDefault(), out var epoch)
                ? DateTimeOffset.FromUnixTimeSeconds(epoch) : null;
            throw new GitHubRateLimitException(reset);
        }
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static void EnsureAllowed(Uri url)
    {
        if (url.Scheme != Uri.UriSchemeHttps || !AllowedHosts.Contains(url.Host))
            throw new InvalidOperationException($"Host not allowed: {url.Host}");
    }
}
