using System.Reflection;
using System.Text.RegularExpressions;

namespace LotusPorts.Catalog;

/// <summary>
/// A translation of a port's "how to play" README section, made for the catalog (catalog/guides/{language}/{portId}.md).
/// <see cref="Fingerprint"/> identifies the English text it was made from, so the app can tell when the authors changed it.
/// </summary>
public sealed record GuideTranslation(string Markdown, string Fingerprint, Uri Source);

public static partial class GuideTranslations
{
    private static readonly Assembly Assembly = typeof(GuideTranslations).Assembly;

    public static GuideTranslation? Find(string portId, string language)
    {
        ArgumentNullException.ThrowIfNull(portId);
        ArgumentNullException.ThrowIfNull(language);
        // MSBuild writes the folder with the system separator in the resource name.
        using var stream = Assembly.GetManifestResourceStream($"guides/{language}/{portId}.md")
            ?? Assembly.GetManifestResourceStream($"guides/{language}\\{portId}.md");
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd().Replace("\r\n", "\n");
        var newline = text.IndexOf('\n');
        if (newline < 0) return null;
        var header = Header().Match(text[..newline]);
        if (!header.Success || !Uri.TryCreate(header.Groups[1].Value, UriKind.Absolute, out var source)) return null;
        return new GuideTranslation(text[(newline + 1)..].Trim() + "\n", header.Groups[2].Value, source);
    }

    [GeneratedRegex(@"^<!--\s*source:\s*(\S+)\s*\|\s*fingerprint:\s*([0-9a-f]{16})")]
    private static partial Regex Header();
}
