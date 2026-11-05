namespace LotusPorts.App;

/// <summary>Small x:Bind helpers.</summary>
public static class Ui
{
    public static bool HasText(string? text) => !string.IsNullOrEmpty(text);

    public static string VersionLabel(string version) => "  " + Strings.Format("SettingsVersion", version);
}
