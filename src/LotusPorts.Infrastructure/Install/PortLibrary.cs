using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LotusPorts.Core.Install;

namespace LotusPorts.Infrastructure.Install;

/// <summary>
/// The installed ports on disk: {library}\{portId}\versions\{version}\ and {library}\{portId}\active.json.
/// New installs go to the chosen library folder; ports installed before a folder change are still found in the old one.
/// </summary>
public sealed partial class PortLibrary(AppPaths paths)
{
    public const string ManifestName = "active.json";
    public const string StagingSuffix = ".staging";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private string? chosen;

    /// <summary>Where new installs go: the folder from Configurações, or %LOCALAPPDATA%\LotusPorts\library.</summary>
    public string Root => chosen ?? paths.DefaultLibrary;

    public void UseFolder(string? folder) => chosen = string.IsNullOrWhiteSpace(folder) ? null : Path.GetFullPath(folder);

    private IEnumerable<string> Roots => new[] { Root, paths.DefaultLibrary }.Distinct(StringComparer.OrdinalIgnoreCase);

    /// <summary>The folder of this port: where it is installed, or where a new install will go.</summary>
    public string PortFolder(string portId)
    {
        EnsureId(portId);
        return Roots.Select(r => Path.Combine(r, portId)).FirstOrDefault(f => File.Exists(Path.Combine(f, ManifestName)) || Directory.Exists(Path.Combine(f, "kept")))
            ?? Path.Combine(Root, portId);
    }

    public string VersionsFolder(string portId) => Path.Combine(PortFolder(portId), "versions");

    public string VersionFolder(string portId, string folder) => Path.Combine(VersionsFolder(portId), folder);

    /// <summary>Saves and settings kept by "Desinstalar mantendo saves", used again by the next install.</summary>
    public string KeptFolder(string portId) => Path.Combine(PortFolder(portId), "kept");

    public InstalledPort? Read(string portId)
    {
        var file = Path.Combine(PortFolder(portId), ManifestName);
        if (!File.Exists(file)) return null;
        try
        {
            var manifest = JsonSerializer.Deserialize<InstalledPort>(File.ReadAllText(file), Json);
            return manifest is { SchemaVersion: InstalledPort.CurrentSchema } m && m.PortId == portId && m.Versions.Any(v => v.Tag == m.ActiveTag) ? m : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public IReadOnlyDictionary<string, InstalledPort> ReadAll()
    {
        var all = new Dictionary<string, InstalledPort>(StringComparer.Ordinal);
        foreach (var root in Roots.Where(Directory.Exists))
            foreach (var folder in Directory.EnumerateDirectories(root))
            {
                var id = Path.GetFileName(folder);
                if (!IdRegex().IsMatch(id) || all.ContainsKey(id)) continue;
                if (Read(id) is { } manifest) all[id] = manifest;
            }
        return all;
    }

    /// <summary>Writes active.json through a temporary file, so a crash never leaves half a manifest.</summary>
    public void Write(InstalledPort manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var folder = PortFolder(manifest.PortId);
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, ManifestName);
        var temp = file + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(manifest, Json));
        File.Move(temp, file, overwrite: true);
    }

    public void DeleteManifest(string portId)
    {
        var file = Path.Combine(PortFolder(portId), ManifestName);
        if (File.Exists(file)) File.Delete(file);
    }

    /// <summary>Unfinished installs (app closed in the middle): their staging folders and partial downloads are removed.</summary>
    public int CleanLeftovers()
    {
        var removed = 0;
        foreach (var root in Roots.Where(Directory.Exists))
            foreach (var port in Directory.EnumerateDirectories(root))
            {
                var versions = Path.Combine(port, "versions");
                if (!Directory.Exists(versions)) continue;
                foreach (var staging in Directory.EnumerateDirectories(versions, "*" + StagingSuffix))
                {
                    DeleteFolder(staging);
                    removed++;
                }
            }
        if (Directory.Exists(paths.DownloadsCache))
            foreach (var part in Directory.EnumerateFiles(paths.DownloadsCache, "*.part", SearchOption.AllDirectories))
            {
                File.Delete(part);
                removed++;
            }
        return removed;
    }

    /// <summary>Deletes a folder, also read-only files inside it. Never follows links out of the folder.</summary>
    public static void DeleteFolder(string folder)
    {
        if (!Directory.Exists(folder)) return;
        var info = new DirectoryInfo(folder);
        if (info.LinkTarget is not null)
        {
            info.Delete();
            return;
        }
        foreach (var entry in info.EnumerateFileSystemInfos("*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0 }))
            if (entry.Attributes.HasFlag(FileAttributes.ReadOnly)) entry.Attributes &= ~FileAttributes.ReadOnly;
        info.Delete(recursive: true);
    }

    public static void EnsureId(string portId)
    {
        if (portId is null || !IdRegex().IsMatch(portId)) throw new ArgumentException("Invalid port id.", nameof(portId));
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,99}$")]
    private static partial Regex IdRegex();
}

/// <summary>Finds the program to open inside a version folder.</summary>
public static class ExecutableFinder
{
    private static readonly string[] NotTheGame = ["unins", "setup", "install", "crash", "vc_redist", "vcredist", "dxsetup", "dxwebsetup", "updater"];

    /// <summary>
    /// The relative path of the program: the catalog's executable when declared (the shallowest copy; two at the same
    /// depth is ambiguous), otherwise the only .exe that is not an installer or helper. Null when it can't be told.
    /// </summary>
    public static string? Find(string folder, string? declared)
    {
        var exes = Directory.EnumerateFiles(folder, "*.exe", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(folder, f))
            .ToList();
        List<string> candidates;
        if (!string.IsNullOrWhiteSpace(declared))
        {
            var normalized = declared.Replace('/', '\\').TrimStart('\\');
            var exact = exes.FirstOrDefault(e => string.Equals(e, normalized, StringComparison.OrdinalIgnoreCase));
            if (exact is not null) return IsWindowsProgram(Path.Combine(folder, exact)) ? exact : null;
            var name = Path.GetFileName(normalized);
            candidates = exes.Where(e => string.Equals(Path.GetFileName(e), name, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            candidates = exes.Where(e => !NotTheGame.Any(n => Path.GetFileName(e).Contains(n, StringComparison.OrdinalIgnoreCase))).ToList();
        }
        if (candidates.Count == 0) return null;
        var depth = candidates.Min(Depth);
        var shallowest = candidates.Where(c => Depth(c) == depth).ToList();
        if (shallowest.Count != 1) return null;
        return IsWindowsProgram(Path.Combine(folder, shallowest[0])) ? shallowest[0] : null;
    }

    private static int Depth(string relative) => relative.Count(c => c == '\\');

    /// <summary>Starts with "MZ", like every Windows program.</summary>
    public static bool IsWindowsProgram(string file)
    {
        Span<byte> head = stackalloc byte[2];
        using var stream = File.OpenRead(file);
        return stream.Read(head) == 2 && head[0] == (byte)'M' && head[1] == (byte)'Z';
    }
}
