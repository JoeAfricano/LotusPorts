using System.Text.RegularExpressions;

namespace LotusPorts.Catalog;

/// <summary>How a release file is recognized as the Windows download (same rules as tools/CatalogScout).</summary>
public static partial class WindowsAssetRules
{
    public static bool IsWindowsAsset(string name) => Windows().IsMatch(name) && !NotWindows().IsMatch(name);

    [GeneratedRegex(@"(?i)(win|windows|x64|x86_64|\.exe$)")]
    private static partial Regex Windows();

    [GeneratedRegex(@"(?i)(linux|mac|osx|darwin|appimage|flatpak|android|switch|arm64|\.apk$|\.dmg$|debug|pdb|symbols|source|\.sha256$|\.sig$)")]
    private static partial Regex NotWindows();
}
