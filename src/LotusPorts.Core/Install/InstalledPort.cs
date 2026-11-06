using LotusPorts.Core.Verification;

namespace LotusPorts.Core.Install;

/// <summary>One downloaded version, kept in library\{portId}\versions\{folder}.</summary>
public sealed record InstalledVersion(
    string Tag,
    string Folder,
    string AssetName,
    long Size,
    string? Sha256,
    CheckState Integrity,
    string Executable,
    DateTimeOffset InstalledAt);

/// <summary>
/// library\{portId}\active.json: which version is active, the one before it (for "Voltar para a versão anterior")
/// and how the first launch after an update went.
/// </summary>
public sealed record InstalledPort(
    int SchemaVersion,
    string PortId,
    string ActiveTag,
    string? PreviousTag,
    IReadOnlyList<InstalledVersion> Versions,
    bool PendingFirstLaunch,
    bool LastLaunchFailed,
    string? PinnedTag = null,
    LastRun? LastRun = null,
    AppliedStyle? Style = null)
{
    public const int CurrentSchema = 1;

    public InstalledVersion Active => Versions.First(v => v.Tag == ActiveTag);

    public InstalledVersion? Previous => PreviousTag is null ? null : Versions.FirstOrDefault(v => v.Tag == PreviousTag);

    public bool CanRollback => Previous is not null;
}

/// <summary>
/// The style applied to the active version: which one, what each key held before (null = it did not exist), the backup of
/// the configuration file taken before writing, and when.
/// </summary>
public sealed record AppliedStyle(string Name, string Tag, IReadOnlyDictionary<string, string?> Previous, string? Backup, DateTimeOffset At);

/// <summary>How the last session went: when, the exit code, how long it ran and whether it was the first after an update.</summary>
public sealed record LastRun(DateTimeOffset At, int ExitCode, double Seconds, bool FirstAfterUpdate, string Tag)
{
    /// <summary>Closed with an error code within 15 seconds: the game did not really open (master prompt, section 10.1).</summary>
    public bool FailedQuickly => ExitCode != 0 && Seconds < 15;
}

/// <summary>Folder names for versions: the release tag, made safe for Windows.</summary>
public static class VersionFolders
{
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    public static string For(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        var invalid = Path.GetInvalidFileNameChars();
        var chars = tag.Select(c => invalid.Contains(c) || c is '/' or '\\' or ':' ? '_' : c).ToArray();
        var name = new string(chars).Trim().Trim('.');
        if (name.Length == 0 || name.All(c => c == '_')) name = "version";
        if (Reserved.Contains(name)) name = "_" + name;
        return name.Length > 80 ? name[..80] : name;
    }
}
