using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Xml.Linq;
using LotusPorts.Catalog;
using LotusPorts.Core.Diagnostics;
using LotusPorts.Core.Install;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Verification;
using LotusPorts.Infrastructure.Install;
using Microsoft.Win32;

namespace LotusPorts.Infrastructure.Diagnostics;

/// <summary>What the app knows about the game file for this port (from the game files search).</summary>
public enum GameFileState
{
    NotRequired,
    Recognized,
    NotFound,
    NotSearched,
}

/// <summary>
/// The local "Resolver" checks (master prompt, section 10.2). Everything here only reads: files, the registry and the
/// Windows event log. A check is OK only when it really ran; when Lotus Ports cannot tell, it says NOT VERIFIED.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PortDiagnostics(PortLibrary library, SystemFolders folders, TimeProvider time)
{
    private const long LowSpace = 2L * 1024 * 1024 * 1024;
    private const long VeryLowSpace = 300L * 1024 * 1024;

    /// <summary>Reads crashes from the event log; replaceable in tests.</summary>
    public Func<string, TimeSpan, IReadOnlyList<CrashRecord>> ReadCrashes { get; set; } = ReadApplicationErrors;

    public ResolveReport Run(LibraryGame game, Port port, InstalledPort? manifest, GameFileState gameFile)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(port);
        var checks = new List<ResolveCheck>();
        var versionFolder = manifest is null ? null : library.VersionFolder(port.Id, manifest.Active.Folder);
        var program = manifest is null || versionFolder is null ? null : Path.Combine(versionFolder, manifest.Active.Executable);

        checks.Add(ProgramCheck(manifest, program));
        checks.Add(GameFileCheck(port, gameFile));
        checks.Add(DependenciesCheck(port));
        checks.Add(GraphicsCheck(port));
        checks.Add(PathCheck(versionFolder));
        checks.Add(DiskCheck(versionFolder ?? library.Root));
        checks.Add(CrashCheck(manifest, program));
        checks.Add(SavesCheck(port, versionFolder));
        checks.Add(UpdateCheck(manifest));
        return new ResolveReport(game.Game.Title, port.Title, manifest?.ActiveTag, time.GetUtcNow(), checks);
    }

    private static ResolveCheck ProgramCheck(InstalledPort? manifest, string? program)
    {
        if (manifest is null || program is null) return ResolveCheck.NotVerified(ResolveCheckKind.Program, "NotInstalled");
        if (!File.Exists(program))
            // An antivirus may have quarantined it, but nothing here proves that: the text only mentions the possibility.
            return new(ResolveCheckKind.Program, CheckState.Failure, "ProgramMissing", [program], ResolveAction.Reinstall);
        return ResolveCheck.Ok(ResolveCheckKind.Program, "ProgramPresent", program);
    }

    private static ResolveCheck GameFileCheck(Port port, GameFileState state) => state switch
    {
        GameFileState.Recognized => ResolveCheck.Ok(ResolveCheckKind.GameFile, "GameFileRecognized"),
        GameFileState.NotRequired => ResolveCheck.NotVerified(ResolveCheckKind.GameFile, "GameFileUnknownNeed"),
        GameFileState.NotFound => new(ResolveCheckKind.GameFile, CheckState.Warning, "GameFileNotFound", [], ResolveAction.AddGameFile),
        _ => new(ResolveCheckKind.GameFile, CheckState.NotVerified, port.Setup.Kind == "select-on-first-launch" ? "GameFileChosenInGame" : "GameFileNotSearched", [], ResolveAction.AddGameFile),
    };

    /// <summary>Visual C++ 2015–2022 x64 is the runtime most ports need; it is checked only when the catalog declares it.</summary>
    private static ResolveCheck DependenciesCheck(Port port)
    {
        var declared = port.Dependencies ?? [];
        if (declared.Count == 0) return ResolveCheck.NotVerified(ResolveCheckKind.Dependencies, "DependenciesNotDeclared");
        var missing = new List<string>();
        var unknown = new List<string>();
        foreach (var dependency in declared)
        {
            switch (dependency)
            {
                case "vcredist-x64" or "vcredist" or "msvc-runtime":
                    if (!VcRuntimeInstalled()) missing.Add("Microsoft Visual C++ 2015–2022 (x64)");
                    break;
                case "vulkan":
                    if (!File.Exists(Path.Combine(Environment.SystemDirectory, "vulkan-1.dll"))) missing.Add("Vulkan");
                    break;
                default:
                    unknown.Add(dependency);
                    break;
            }
        }
        if (missing.Count > 0) return new(ResolveCheckKind.Dependencies, CheckState.Failure, "DependenciesMissing", [string.Join(", ", missing)]);
        if (unknown.Count > 0) return ResolveCheck.NotVerified(ResolveCheckKind.Dependencies, "DependenciesUnknown", string.Join(", ", unknown));
        return ResolveCheck.Ok(ResolveCheckKind.Dependencies, "DependenciesPresent");
    }

    private static bool VcRuntimeInstalled()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64");
        return key?.GetValue("Installed") is int installed && installed == 1;
    }

    /// <summary>Only whether Windows has the graphics libraries the port uses; whether the graphics card supports them is not checked here.</summary>
    private static ResolveCheck GraphicsCheck(Port port)
    {
        var apis = port.GraphicsApis ?? [];
        if (apis.Count == 0) return ResolveCheck.NotVerified(ResolveCheckKind.Graphics, "GraphicsNotDeclared");
        var system = Environment.SystemDirectory;
        var present = apis.Where(api => api switch
        {
            "Vulkan" => File.Exists(Path.Combine(system, "vulkan-1.dll")),
            "D3D12" => File.Exists(Path.Combine(system, "d3d12.dll")),
            "D3D11" => File.Exists(Path.Combine(system, "d3d11.dll")),
            "OpenGL" => File.Exists(Path.Combine(system, "opengl32.dll")),
            _ => false,
        }).ToList();
        if (present.Count == 0) return new(ResolveCheckKind.Graphics, CheckState.Failure, "GraphicsMissing", [string.Join(", ", apis)]);
        // At least one way to draw exists in Windows; the card's own support stays NOT VERIFIED without Lotus.
        return new(ResolveCheckKind.Graphics, CheckState.NotVerified, "GraphicsLibrariesPresent", [string.Join(", ", present)]);
    }

    private ResolveCheck PathCheck(string? versionFolder)
    {
        if (versionFolder is null || !Directory.Exists(versionFolder)) return ResolveCheck.NotVerified(ResolveCheckKind.Path, "NotInstalled");
        if (folders.Synced.Any(s => versionFolder.StartsWith(s.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase)))
            return new(ResolveCheckKind.Path, CheckState.Warning, "PathOneDrive", [versionFolder], ResolveAction.OpenFolder);
        if (versionFolder.Length > 200) return new(ResolveCheckKind.Path, CheckState.Warning, "PathLong", [versionFolder.Length.ToString(CultureInfo.InvariantCulture)]);
        if (versionFolder.Any(c => c > 127)) return new(ResolveCheckKind.Path, CheckState.Warning, "PathSpecialCharacters", [versionFolder]);
        if (!CanWrite(versionFolder)) return new(ResolveCheckKind.Path, CheckState.Failure, "PathNotWritable", [versionFolder], ResolveAction.OpenFolder);
        return ResolveCheck.Ok(ResolveCheckKind.Path, "PathOk", versionFolder);
    }

    /// <summary>Writes and deletes a tiny file in the game's own version folder (inside the Lotus Ports library).</summary>
    private static bool CanWrite(string folder)
    {
        var probe = Path.Combine(folder, $".lotusports-write-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllBytes(probe, []);
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static ResolveCheck DiskCheck(string folder)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(folder));
            if (root is null) return ResolveCheck.NotVerified(ResolveCheckKind.DiskSpace, "DiskUnknown");
            var free = new DriveInfo(root).AvailableFreeSpace;
            var gb = (free / (1024.0 * 1024 * 1024)).ToString("0.0", CultureInfo.CurrentCulture);
            return free switch
            {
                < VeryLowSpace => new(ResolveCheckKind.DiskSpace, CheckState.Failure, "DiskVeryLow", [root, gb]),
                < LowSpace => new(ResolveCheckKind.DiskSpace, CheckState.Warning, "DiskLow", [root, gb]),
                _ => ResolveCheck.Ok(ResolveCheckKind.DiskSpace, "DiskOk", root, gb),
            };
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException)
        {
            return ResolveCheck.NotVerified(ResolveCheckKind.DiskSpace, "DiskUnknown");
        }
    }

    private ResolveCheck CrashCheck(InstalledPort? manifest, string? program)
    {
        if (manifest is null || program is null) return ResolveCheck.NotVerified(ResolveCheckKind.LastCrash, "NotInstalled");
        IReadOnlyList<CrashRecord> crashes;
        try { crashes = ReadCrashes(Path.GetFileName(program), TimeSpan.FromDays(7)); }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception or System.Xml.XmlException)
        {
            return ResolveCheck.NotVerified(ResolveCheckKind.LastCrash, "CrashLogUnreadable");
        }
        var run = manifest.LastRun;
        if (crashes.FirstOrDefault() is { } crash)
        {
            var when = crash.At.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
            return new(ResolveCheckKind.LastCrash, CheckState.Failure, "CrashRecorded", [when, crash.FaultingModule, crash.ExceptionCode]);
        }
        if (run is { FailedQuickly: true })
            return new(ResolveCheckKind.LastCrash, CheckState.Warning, "ClosedWithError",
                [run.ExitCode.ToString(CultureInfo.InvariantCulture), $"0x{run.ExitCode:X8}", run.Seconds.ToString("0.#", CultureInfo.CurrentCulture)]);
        return run is null
            ? ResolveCheck.NotVerified(ResolveCheckKind.LastCrash, "NeverRun")
            : ResolveCheck.Ok(ResolveCheckKind.LastCrash, "NoCrash");
    }

    private static ResolveCheck SavesCheck(Port port, string? versionFolder)
    {
        var paths = port.PersistentPaths.Where(p => p.Kind is "saves" or "config").ToList();
        if (paths.Count == 0) return ResolveCheck.NotVerified(ResolveCheckKind.Saves, "SavesNotDeclared");
        var readOnly = new List<string>();
        var checkedAny = false;
        foreach (var entry in paths)
        {
            var path = entry.Location == "user" ? Environment.ExpandEnvironmentVariables(entry.Path)
                : versionFolder is null ? null : Path.Combine(versionFolder, entry.Path.Replace('/', '\\').TrimEnd('\\'));
            if (path is null) continue;
            if (Directory.Exists(path))
            {
                checkedAny = true;
                if (new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.ReadOnly)) readOnly.Add(path);
            }
            else if (File.Exists(path))
            {
                checkedAny = true;
                if (new FileInfo(path).IsReadOnly) readOnly.Add(path);
            }
        }
        if (readOnly.Count > 0) return new(ResolveCheckKind.Saves, CheckState.Warning, "SavesReadOnly", [string.Join("; ", readOnly)]);
        // Folders that don't exist yet are created by the game itself on the first run.
        return checkedAny ? ResolveCheck.Ok(ResolveCheckKind.Saves, "SavesWritable") : ResolveCheck.NotVerified(ResolveCheckKind.Saves, "SavesNotCreatedYet");
    }

    private static ResolveCheck UpdateCheck(InstalledPort? manifest)
    {
        if (manifest is null) return ResolveCheck.NotVerified(ResolveCheckKind.RecentUpdate, "NotInstalled");
        if (manifest is { LastRun: { FirstAfterUpdate: true, FailedQuickly: true }, Previous: { } previous })
            return new(ResolveCheckKind.RecentUpdate, CheckState.Failure, "ProblemAfterUpdate", [manifest.ActiveTag, previous.Tag], ResolveAction.Rollback);
        if (manifest.Previous is { } prev && manifest.LastLaunchFailed)
            return new(ResolveCheckKind.RecentUpdate, CheckState.Warning, "PreviousVersionAvailable", [prev.Tag], ResolveAction.Rollback);
        return ResolveCheck.Ok(ResolveCheckKind.RecentUpdate, manifest.Previous is null ? "NoRecentUpdate" : "UpdateNotTheCause");
    }

    /// <summary>
    /// Application Error (event 1000) entries for the program in the last days, newest first, read with the built-in
    /// wevtutil. Only the program name, faulting module and exception code are kept.
    /// </summary>
    public static IReadOnlyList<CrashRecord> ReadApplicationErrors(string programName, TimeSpan within)
    {
        var ms = (long)within.TotalMilliseconds;
        var query = $"*[System[Provider[@Name='Application Error'] and (EventID=1000) and TimeCreated[timediff(@SystemTime) <= {ms}]]]";
        var info = new ProcessStartInfo("wevtutil.exe")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var arg in new[] { "qe", "Application", $"/q:{query}", "/c:50", "/rd:true", "/f:xml" }) info.ArgumentList.Add(arg);
        using var process = Process.Start(info) ?? throw new InvalidOperationException("wevtutil did not start.");
        var xml = process.StandardOutput.ReadToEnd();
        if (!process.WaitForExit(10_000)) { process.Kill(); throw new InvalidOperationException("wevtutil took too long."); }
        return ParseApplicationErrors(xml, programName);
    }

    /// <summary>Parses wevtutil's XML (several Event elements without a root).</summary>
    public static IReadOnlyList<CrashRecord> ParseApplicationErrors(string xml, string programName)
    {
        if (string.IsNullOrWhiteSpace(xml)) return [];
        var root = XElement.Parse($"<Events>{xml}</Events>");
        XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
        var list = new List<CrashRecord>();
        foreach (var e in root.Elements(ns + "Event"))
        {
            var data = e.Element(ns + "EventData")?.Elements(ns + "Data").Select(d => d.Value).ToList() ?? [];
            // Application Error data: 0 app name, 1 app version, 2 app timestamp, 3 module, 4 module version, 5 module timestamp, 6 exception code.
            if (data.Count < 7 || !string.Equals(data[0], programName, StringComparison.OrdinalIgnoreCase)) continue;
            var created = e.Element(ns + "System")?.Element(ns + "TimeCreated")?.Attribute("SystemTime")?.Value;
            var at = DateTimeOffset.TryParse(created, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var t) ? t : DateTimeOffset.MinValue;
            var code = data[6].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? data[6] : "0x" + data[6];
            list.Add(new CrashRecord(at, data[0], data[3], code));
        }
        return list.OrderByDescending(c => c.At).ToList();
    }
}
