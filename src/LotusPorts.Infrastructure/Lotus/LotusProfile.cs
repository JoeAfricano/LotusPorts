using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Win32;

namespace LotusPorts.Infrastructure.Lotus;

/// <summary>The player profile created in Lotus (name, picture, accent color).</summary>
public sealed record LotusProfile(string DisplayName, string? AvatarPath, string? Accent);

/// <summary>Lotus's theme choice: mode (Midnight/Light/System/Graphite/Custom), accent and the custom theme's share code.</summary>
public sealed record LotusAppearance(string Theme, string Accent, string? CustomTheme);

/// <summary>
/// Reads the player profile from Lotus's own preferences (%LOCALAPPDATA%\Lotus\preferences.json) and finds
/// the installed Lotus. Read-only: Lotus Ports never writes to Lotus's files or registry.
/// </summary>
public sealed class LotusProfileReader(string? lotusDataFolder = null)
{
    /// <summary>Where people get Lotus when it is not installed.</summary>
    public static readonly Uri DownloadPage = new("https://github.com/JoeAfricano/Lotus/releases/latest");

    private static readonly string[] PictureExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp"];

    private string Folder => lotusDataFolder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lotus");

    /// <summary>The profile, or null when Lotus is missing, the welcome was never finished or the file can't be read.</summary>
    public LotusProfile? Read()
    {
        var file = Path.Combine(Folder, "preferences.json");
        try
        {
            if (!File.Exists(file) || new FileInfo(file).Length > 1024 * 1024) return null;
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            if (!(root.TryGetProperty("Onboarded", out var onboarded) && onboarded.ValueKind == JsonValueKind.True)) return null;
            var name = Text(root, "DisplayName")?.Trim();
            if (string.IsNullOrEmpty(name)) return null;
            if (name.Length > 40) name = name[..40];
            var avatar = Text(root, "AvatarPath");
            if (avatar is not null && !(Path.IsPathFullyQualified(avatar) && PictureExtensions.Contains(Path.GetExtension(avatar).ToLowerInvariant()) && File.Exists(avatar)))
                avatar = null;
            return new LotusProfile(name, avatar, Text(root, "Accent"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    /// <summary>Lotus's look (Theme, Accent, CustomTheme from its preferences), so Lotus Ports can match it. Null without Lotus.</summary>
    public LotusAppearance? ReadAppearance()
    {
        var file = Path.Combine(Folder, "preferences.json");
        try
        {
            if (!File.Exists(file) || new FileInfo(file).Length > 1024 * 1024) return null;
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;
            return new LotusAppearance(Text(root, "Theme") ?? "Midnight", Text(root, "Accent") ?? "rose", Text(root, "CustomTheme"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    private static string? Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    /// <summary>Lotus.App.exe from Lotus's uninstall entry (current user first), or null when Lotus is not installed.</summary>
    [SupportedOSPlatform("windows")]
    public static string? FindLotusExecutable()
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var key = hive.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key is null) continue;
            foreach (var sub in key.GetSubKeyNames())
            {
                using var app = key.OpenSubKey(sub);
                if (app?.GetValue("DisplayName") as string != "Lotus") continue;
                var folder = app.GetValue("InstallLocation") as string;
                if (string.IsNullOrWhiteSpace(folder)) continue;
                var exe = Path.Combine(folder, "Lotus.App.exe");
                if (File.Exists(exe)) return exe;
            }
        }
        return null;
    }

    /// <summary>Opens Lotus (it shows its welcome when there is no profile yet) or, without Lotus, its download page.</summary>
    [SupportedOSPlatform("windows")]
    public static bool OpenLotus()
    {
        if (FindLotusExecutable() is not { } exe) return false;
        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(exe)! })?.Dispose();
        return true;
    }
}
