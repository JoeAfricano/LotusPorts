using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Catalog;

/// <summary>
/// Box art fetched when shown and kept on disk (%LOCALAPPDATA%\LotusPorts\cache\covers). Only PNG files from
/// GitHub are accepted; each cover is downloaded once, at most four at a time. Failures fall back to the
/// generated cover and are retried on a later visit.
/// </summary>
public sealed class CoverCache(AppPaths paths, GitHubClient github, ILogger<CoverCache> logger)
{
    private const long MaxBytes = 4 * 1024 * 1024;
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly ConcurrentDictionary<string, Task<string?>> pending = new();
    private readonly SemaphoreSlim gate = new(4);

    private string Folder => Path.Combine(paths.Root, "cache", "covers");

    /// <summary>
    /// Set by the app: shrinks a downloaded PNG to a small JPEG (cover size) to keep the cache light.
    /// Without it the original PNG is kept.
    /// </summary>
    public Func<byte[], Task<byte[]?>>? Shrink { get; set; }

    /// <summary>The local file for this cover, downloading it the first time. Null when unavailable.</summary>
    public Task<string?> GetAsync(string url)
    {
        var file = Path.Combine(Folder, Hash(url) + (Shrink is null ? ".png" : ".jpg"));
        if (File.Exists(file)) return Task.FromResult<string?>(file);
        return pending.GetOrAdd(url, u => FetchAsync(u, file));
    }

    private async Task<string?> FetchAsync(string url, string file)
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var bytes = await github.GetBytesAsync(new Uri(url), MaxBytes).ConfigureAwait(false);
            if (bytes is null || bytes.Length < PngSignature.Length || !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
                return null;
            if (Shrink is not null) bytes = await Shrink(bytes).ConfigureAwait(false);
            if (bytes is null) return null;
            Directory.CreateDirectory(Folder);
            var temp = file + ".part";
            await File.WriteAllBytesAsync(temp, bytes).ConfigureAwait(false);
            File.Move(temp, file, overwrite: true);
            return file;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidDataException or IOException or InvalidOperationException)
        {
            logger.LogDebug(ex, "Cover not available");
            return null;
        }
        finally
        {
            gate.Release();
            pending.TryRemove(url, out _);
        }
    }

    private static string Hash(string url) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(url)))[..32];
}
