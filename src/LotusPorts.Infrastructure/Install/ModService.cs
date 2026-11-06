using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;

namespace LotusPorts.Infrastructure.Install;

public enum ModState
{
    Enabled,
    Disabled,
}

public sealed record ModFile(string Name, long Size, DateTime ModifiedUtc, ModState State);

public enum ModProblem
{
    NotOffered,
    NotInstalled,
    WrongType,
    TooLarge,
    NotFound,
}

public sealed class ModException(ModProblem problem, string message) : Exception(message)
{
    public ModProblem Problem { get; } = problem;
}

/// <summary>
/// Mods (Phase 8, experimental), only for ports whose own repository declares how they take mods. Lotus Ports never
/// downloads mods: the player picks a file they downloaded from the official page. Nothing is ever deleted — a
/// replaced or removed mod goes to library\{portId}\mod-backups — and every change is journaled BEFORE/AFTER.
/// Disabled mods wait in library\{portId}\mods-disabled, outside the folder the game reads.
/// </summary>
public sealed class ModService(PortLibrary library, IOperationJournal journal, TimeProvider time)
{
    public const long MaxBytes = 4L * 1024 * 1024 * 1024;
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    public static bool Offers(Port port) => port?.Mods is not null;

    /// <summary>The folder the game reads mods from, or null when it cannot be known (not installed for in-game folders).</summary>
    public string? ModsFolder(Port port, InstalledPort? manifest)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (port.Mods is not { } mods) return null;
        if (mods.Location == "user") return Path.GetFullPath(Environment.ExpandEnvironmentVariables(mods.Folder.Replace('/', '\\')));
        if (manifest is null) return null;
        return Path.Combine(library.VersionFolder(port.Id, manifest.Active.Folder), mods.Folder.Replace('/', '\\').TrimEnd('\\'));
    }

    public string DisabledFolder(Port port) => Path.Combine(library.PortFolder(port.Id), "mods-disabled");

    public string BackupRoot(Port port) => Path.Combine(library.PortFolder(port.Id), "mod-backups");

    public IReadOnlyList<ModFile> List(Port port, InstalledPort? manifest)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (port.Mods is not { } mods) return [];
        var types = mods.FileTypes.Select(t => t.ToLowerInvariant()).ToHashSet();
        IEnumerable<ModFile> Read(string? folder, ModState state) => folder is null || !Directory.Exists(folder) ? []
            : new DirectoryInfo(folder).EnumerateFiles().Where(f => types.Contains(f.Extension.ToLowerInvariant()))
                .Select(f => new ModFile(f.Name, f.Length, f.LastWriteTimeUtc, state));
        return Read(ModsFolder(port, manifest), ModState.Enabled).Concat(Read(DisabledFolder(port), ModState.Disabled))
            .OrderBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    /// <summary>Copies a mod the player picked into the game's mods folder. The picked file is only read.</summary>
    public async Task<ModFile> InstallAsync(Port port, InstalledPort manifest, string sourceFile, CancellationToken cancellationToken = default)
    {
        var folder = Folder(port, manifest);
        var info = new FileInfo(sourceFile);
        if (!info.Exists) throw new ModException(ModProblem.NotFound, "File not found.");
        if (!port.Mods!.FileTypes.Contains(info.Extension.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase))
            throw new ModException(ModProblem.WrongType, "This port does not take this kind of file.");
        if (info.Length > MaxBytes) throw new ModException(ModProblem.TooLarge, "The file is too large.");

        var target = Path.Combine(folder, info.Name);
        var disabled = Path.Combine(DisabledFolder(port), info.Name);
        ModFile? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.ModInstall, port.Id, State(port, manifest, info.Name), async ct =>
        {
            Directory.CreateDirectory(folder);
            // A mod with the same name (enabled or disabled) is kept in a backup, never overwritten.
            var replaced = File.Exists(target) ? Backup(port, target) : File.Exists(disabled) ? Backup(port, disabled) : null;
            var temp = target + ".lotusports-part";
            await using (var input = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true))
            await using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                await input.CopyToAsync(output, ct).ConfigureAwait(false);
            File.Move(temp, target, overwrite: true);
            var written = new FileInfo(target);
            result = new ModFile(written.Name, written.Length, written.LastWriteTimeUtc, ModState.Enabled);
            return new OperationStepResult(OperationOutcome.Success, State(port, manifest, info.Name), replaced is null ? info.Name : $"{info.Name} (anterior guardado)");
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result!;
    }

    /// <summary>Moves a mod out of the folder the game reads (it stays on the PC, ready to come back).</summary>
    public Task DisableAsync(Port port, InstalledPort manifest, string name) =>
        MoveAsync(port, manifest, name, OperationKind.ModDisable, Path.Combine(Folder(port, manifest), Safe(name)), Path.Combine(DisabledFolder(port), Safe(name)));

    public Task EnableAsync(Port port, InstalledPort manifest, string name) =>
        MoveAsync(port, manifest, name, OperationKind.ModEnable, Path.Combine(DisabledFolder(port), Safe(name)), Path.Combine(Folder(port, manifest), Safe(name)));

    /// <summary>Removes a mod by moving it into a dated backup folder (never deleted).</summary>
    public async Task<string> RemoveAsync(Port port, InstalledPort manifest, string name)
    {
        var enabled = Path.Combine(Folder(port, manifest), Safe(name));
        var disabled = Path.Combine(DisabledFolder(port), Safe(name));
        var source = File.Exists(enabled) ? enabled : File.Exists(disabled) ? disabled : throw new ModException(ModProblem.NotFound, "Mod not found.");
        string? backup = null;
        await JournaledOperation.RunAsync(journal, OperationKind.ModRemove, port.Id, State(port, manifest, name), _ =>
        {
            backup = Backup(port, source);
            return Task.FromResult(new OperationStepResult(OperationOutcome.Success, State(port, manifest, name), name));
        }).ConfigureAwait(false);
        return backup!;
    }

    private async Task MoveAsync(Port port, InstalledPort manifest, string name, OperationKind kind, string from, string to)
    {
        if (!File.Exists(from)) throw new ModException(ModProblem.NotFound, "Mod not found.");
        await JournaledOperation.RunAsync(journal, kind, port.Id, State(port, manifest, name), _ =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(to)!);
            if (File.Exists(to)) Backup(port, to);
            File.Move(from, to);
            return Task.FromResult(new OperationStepResult(OperationOutcome.Success, State(port, manifest, name), name));
        }).ConfigureAwait(false);
    }

    private string Backup(Port port, string file)
    {
        var folder = Path.Combine(BackupRoot(port), time.GetUtcNow().ToString("yyyyMMdd-HHmmss-fff", System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, Path.GetFileName(file));
        File.Move(file, target);
        return target;
    }

    private string Folder(Port port, InstalledPort manifest)
    {
        if (port.Mods is null) throw new ModException(ModProblem.NotOffered, "This port does not declare mod support.");
        return ModsFolder(port, manifest) ?? throw new ModException(ModProblem.NotInstalled, "Not installed.");
    }

    /// <summary>Only a plain file name: no folders, no "..".</summary>
    private static string Safe(string name)
    {
        var file = Path.GetFileName(name);
        if (string.IsNullOrWhiteSpace(file) || file != name || file is "." or "..") throw new ModException(ModProblem.NotFound, "Invalid mod name.");
        return file;
    }

    private string State(Port port, InstalledPort manifest, string name) =>
        JsonSerializer.Serialize(new { mod = name, mods = List(port, manifest).Select(m => new { m.Name, State = m.State.ToString() }) }, StateJson);
}
