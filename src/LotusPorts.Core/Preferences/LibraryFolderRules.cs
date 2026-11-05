namespace LotusPorts.Core.Preferences;

public enum LibraryFolderProblem
{
    None,
    NotAbsolute,
    DriveRoot,
    ProtectedSystemFolder,
    SyncedByOneDrive,
}

/// <summary>Folders the library may not live in: protected by Windows or synced by OneDrive.</summary>
public sealed record SystemFolders(IReadOnlyList<string> Protected, IReadOnlyList<string> Synced)
{
    public static SystemFolders FromEnvironment()
    {
        static List<string> Present(params string?[] paths) =>
            paths.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return new(
            Present(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.Windows)),
            Present(
                Environment.GetEnvironmentVariable("OneDrive"),
                Environment.GetEnvironmentVariable("OneDriveConsumer"),
                Environment.GetEnvironmentVariable("OneDriveCommercial")));
    }
}

public static class LibraryFolderRules
{
    public static LibraryFolderProblem Check(string path, SystemFolders folders)
    {
        ArgumentNullException.ThrowIfNull(folders);
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)) return LibraryFolderProblem.NotAbsolute;
        var full = Normalize(Path.GetFullPath(path));
        if (string.Equals(full, Normalize(Path.GetPathRoot(full) ?? ""), StringComparison.OrdinalIgnoreCase)) return LibraryFolderProblem.DriveRoot;
        if (folders.Protected.Any(p => IsInside(full, p))) return LibraryFolderProblem.ProtectedSystemFolder;
        if (folders.Synced.Any(p => IsInside(full, p))) return LibraryFolderProblem.SyncedByOneDrive;
        return LibraryFolderProblem.None;
    }

    private static string Normalize(string path) => path.TrimEnd('\\', '/');

    private static bool IsInside(string path, string folder)
    {
        var f = Normalize(folder);
        return path.Equals(f, StringComparison.OrdinalIgnoreCase) || path.StartsWith(f + "\\", StringComparison.OrdinalIgnoreCase);
    }
}
