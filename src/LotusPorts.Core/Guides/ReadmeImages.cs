using System.Text.RegularExpressions;

namespace LotusPorts.Core.Guides;

/// <summary>
/// The pictures a port shows in its own README (screenshots), for a small gallery on the game page. Only addresses are
/// read; the app loads them when shown, like box art, and nothing is bundled. Badges, logos and icons are left out.
/// </summary>
public static partial class ReadmeImages
{
    public const int Max = 6;

    private static readonly string[] AllowedHosts =
        ["raw.githubusercontent.com", "user-images.githubusercontent.com", "github.com", "private-user-images.githubusercontent.com", "i.imgur.com"];

    private static readonly string[] Skip = ["badge", "shields.io", "logo", "icon", "banner", "button", "discord", "kofi", "ko-fi", "patreon", "paypal", "star-history", "contrib.rocks"];

    /// <param name="rawBase">Base for relative paths, e.g. https://raw.githubusercontent.com/owner/repo/HEAD/.</param>
    public static IReadOnlyList<Uri> Extract(string markdown, Uri rawBase)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(rawBase);
        var found = new List<Uri>();
        foreach (Match m in ImageRegex().Matches(markdown))
        {
            var raw = (m.Groups["md"].Success ? m.Groups["md"].Value : m.Groups["html"].Value).Trim().Trim('<', '>');
            if (raw.Length == 0 || raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
            if (!Uri.TryCreate(raw, UriKind.Absolute, out var url) && !Uri.TryCreate(rawBase, raw.TrimStart('.', '/'), out url)) continue;
            if (url.Scheme != Uri.UriSchemeHttps || !AllowedHosts.Contains(url.Host, StringComparer.OrdinalIgnoreCase)) continue;
            var lower = url.AbsoluteUri.ToLowerInvariant();
            if (Skip.Any(lower.Contains) || lower.EndsWith(".svg", StringComparison.Ordinal)) continue;
            // github.com/owner/repo/blob/... pages are turned into the file itself.
            if (url.Host == "github.com" && url.AbsolutePath.Contains("/blob/", StringComparison.Ordinal))
                url = new Uri(url.AbsoluteUri.Replace("/blob/", "/raw/", StringComparison.Ordinal));
            var isAsset = url.Host == "github.com" && url.AbsolutePath.Contains("/assets/", StringComparison.Ordinal);
            var isImageFile = ImageExtension().IsMatch(url.AbsolutePath);
            if (!isAsset && !isImageFile) continue;
            if (!found.Contains(url)) found.Add(url);
            if (found.Count == Max) break;
        }
        return found;
    }

    [GeneratedRegex("""!\[[^\]]*\]\((?<md>[^)\s]+)(?:\s+"[^"]*")?\)|<img[^>]*?\ssrc\s*=\s*["'](?<html>[^"']+)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex ImageRegex();

    [GeneratedRegex(@"\.(png|jpe?g|gif|webp)$", RegexOptions.IgnoreCase)]
    private static partial Regex ImageExtension();
}
