namespace LotusPorts.Infrastructure;

/// <summary>Folders under %LOCALAPPDATA%\LotusPorts. Only data and logs are created at startup; the rest when first used.</summary>
public sealed class AppPaths(string root)
{
    public string Root { get; } = root;
    public string Data => Path.Combine(Root, "data");
    public string Logs => Path.Combine(Root, "logs");
    public string DownloadsCache => Path.Combine(Root, "cache", "downloads");
    public string DefaultLibrary => Path.Combine(Root, "library");
    public string DatabaseFile => Path.Combine(Data, "lotusports.db");

    public static AppPaths ForCurrentUser() =>
        new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LotusPorts"));

    public void EnsureStartupFolders()
    {
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Logs);
    }
}
