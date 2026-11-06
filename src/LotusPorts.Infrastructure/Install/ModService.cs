using System.IO.Compression;
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

/// <summary>A mod in the game's folder (or waiting, disabled). IsFolder: a mod made of a folder (e.g. one with mod.ini).</summary>
public sealed record ModFile(string Name, long Size, DateTime ModifiedUtc, ModState State, bool IsFolder = false);

public enum ModProblem
{
    NotOffered,
    NotInstalled,
    WrongType,
    TooLarge,
    NotFound,

    /// <summary>A folder or .zip without the file the port uses to recognize a mod (e.g. mod.ini).</summary>
    NotAMod,
}

public sealed class ModException(ModProblem problem, string message) : Exception(message)
{
    public ModProblem Problem { get; } = problem;
}

/// <summary>
/// Mods (Phase 8, experimental), only for ports whose own repository declares how they take mods. Lotus Ports never
/// downloads mods: the player picks a file, a folder or a downloaded .zip. Nothing is ever deleted — a replaced or
/// removed mod goes to library\{portId}\mod-backups — and every change is journaled BEFORE/AFTER. Disabled mods wait
/// in library\{portId}\mods-disabled, outside the folder the game reads.
/// </summary>
public sealed class ModService(PortLibrary library, IOperationJournal journal, TimeProvider time)
{
    public const long MaxBytes = 4L * 1024 * 1024 * 1024;
    private const int MaxEntries = 20_000;
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    public static bool Offers(Port port) => port?.Mods is not null;

    /// <summary>The port takes mods that are folders (recognized by its marker file).</summary>
    public static bool TakesFolders(Port port) => port?.Mods?.FolderMarker is not null;

    /// <summary>A .zip is unpacked as a folder mod (when the port does not take .zip files as mods themselves).</summary>
    public static bool UnpacksZip(Port port) => TakesFolders(port) && !port.Mods!.FileTypes.Contains(".zip", StringComparer.OrdinalIgnoreCase);

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
        IEnumerable<ModFile> Read(string? folder, ModState state)
        {
            if (folder is null || !Directory.Exists(folder)) return [];
            var dir = new DirectoryInfo(folder);
            var files = dir.EnumerateFiles().Where(f => types.Contains(f.Extension.ToLowerInvariant()))
                .Select(f => new ModFile(f.Name, f.Length, f.LastWriteTimeUtc, state));
            var folders = mods.FolderMarker is not { } marker ? [] : dir.EnumerateDirectories()
                .Where(d => File.Exists(Path.Combine(d.FullName, marker)))
                .Select(d => new ModFile(d.Name, FolderSize(d), d.LastWriteTimeUtc, state, IsFolder: true));
            return files.Concat(folders);
        }
        return Read(ModsFolder(port, manifest), ModState.Enabled).Concat(Read(DisabledFolder(port), ModState.Disabled))
            .OrderBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    /// <summary>
    /// Copies a mod the player picked into the game's mods folder. The picked file is only read. For ports that take
    /// folder mods, a .zip is unpacked as a folder (its root, or its single top folder, must have the marker file).
    /// </summary>
    public async Task<ModFile> InstallAsync(Port port, InstalledPort manifest, string sourceFile, CancellationToken cancellationToken = default)
    {
        var folder = Folder(port, manifest);
        var info = new FileInfo(sourceFile);
        if (!info.Exists) throw new ModException(ModProblem.NotFound, "File not found.");
        var extension = info.Extension.ToLowerInvariant();
        if (extension == ".zip" && UnpacksZip(port)) return await InstallZipAsync(port, manifest, info, cancellationToken).ConfigureAwait(false);
        if (!port.Mods!.FileTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new ModException(ModProblem.WrongType, "This port does not take this kind of file.");
        if (info.Length > MaxBytes) throw new ModException(ModProblem.TooLarge, "The file is too large.");

        var target = Path.Combine(folder, info.Name);
        var disabled = Path.Combine(DisabledFolder(port), info.Name);
        ModFile? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.ModInstall, port.Id, State(port, manifest, info.Name), async ct =>
        {
            Directory.CreateDirectory(folder);
            // A mod with the same name (enabled or disabled) is kept in a backup, never overwritten.
            var replaced = BackupExisting(port, target, disabled);
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

    /// <summary>Copies a mod that is a folder (with the port's marker file at its root). The picked folder is only read.</summary>
    public async Task<ModFile> InstallFolderAsync(Port port, InstalledPort manifest, string sourceFolder, CancellationToken cancellationToken = default)
    {
        var folder = Folder(port, manifest);
        var source = new DirectoryInfo(sourceFolder);
        if (!source.Exists) throw new ModException(ModProblem.NotFound, "Folder not found.");
        if (port.Mods!.FolderMarker is not { } marker) throw new ModException(ModProblem.WrongType, "This port does not take folder mods.");
        if (!File.Exists(Path.Combine(source.FullName, marker))) throw new ModException(ModProblem.NotAMod, $"The folder has no {marker}.");
        CheckSize(source.EnumerateFiles("*", SearchOption.AllDirectories).Select(f => f.Length));
        return await CopyFolderAsync(port, manifest, folder, source.Name, (target, ct) => CopyTreeAsync(source.FullName, target, ct), cancellationToken).ConfigureAwait(false);
    }

    private async Task<ModFile> InstallZipAsync(Port port, InstalledPort manifest, FileInfo zip, CancellationToken cancellationToken)
    {
        var folder = Folder(port, manifest);
        var marker = port.Mods!.FolderMarker!;
        string prefix, name;
        using (var archive = ZipFile.OpenRead(zip.FullName))
        {
            var entries = archive.Entries.Where(e => e.Name.Length > 0).ToList();
            if (entries.Count > MaxEntries) throw new ModException(ModProblem.TooLarge, "Too many files.");
            CheckSize(entries.Select(e => e.Length));
            var names = entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
            var tops = names.Select(n => n.Split('/')[0]).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (names.Any(n => n.Equals(marker, StringComparison.OrdinalIgnoreCase)))
                (prefix, name) = ("", Path.GetFileNameWithoutExtension(zip.Name));
            else if (tops.Count == 1 && names.Any(n => n.Equals($"{tops[0]}/{marker}", StringComparison.OrdinalIgnoreCase)))
                (prefix, name) = (tops[0] + "/", tops[0]);
            else throw new ModException(ModProblem.NotAMod, $"The .zip has no {marker}.");
        }
        name = Safe(name.Trim());
        return await CopyFolderAsync(port, manifest, folder, name, async (target, ct) =>
        {
            using var archive = ZipFile.OpenRead(zip.FullName);
            var root = Path.GetFullPath(target).TrimEnd('\\') + "\\";
            foreach (var entry in archive.Entries)
            {
                ct.ThrowIfCancellationRequested();
                var path = entry.FullName.Replace('\\', '/');
                if (entry.Name.Length == 0 || !path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                var destination = Path.GetFullPath(Path.Combine(root, path[prefix.Length..].Replace('/', '\\')));
                // Never outside the mod's own folder (no "..", no rooted names).
                if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ModException(ModProblem.NotAMod, "Unsafe path in the .zip.");
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                await using var input = entry.Open();
                await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true);
                await input.CopyToAsync(output, ct).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ModFile> CopyFolderAsync(Port port, InstalledPort manifest, string modsFolder, string name, Func<string, CancellationToken, Task> fill, CancellationToken cancellationToken)
    {
        name = Safe(name);
        var target = Path.Combine(modsFolder, name);
        var disabled = Path.Combine(DisabledFolder(port), name);
        ModFile? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.ModInstall, port.Id, State(port, manifest, name), async ct =>
        {
            Directory.CreateDirectory(modsFolder);
            var temp = target + ".lotusports-part";
            if (Directory.Exists(temp)) Directory.Delete(temp, recursive: true); // only our own unfinished copy
            Directory.CreateDirectory(temp);
            try { await fill(temp, ct).ConfigureAwait(false); }
            catch
            {
                try { Directory.Delete(temp, recursive: true); } catch (IOException) { }
                throw;
            }
            var replaced = BackupExisting(port, target, disabled);
            Directory.Move(temp, target);
            var written = new DirectoryInfo(target);
            result = new ModFile(name, FolderSize(written), written.LastWriteTimeUtc, ModState.Enabled, IsFolder: true);
            return new OperationStepResult(OperationOutcome.Success, State(port, manifest, name), replaced is null ? name : $"{name} (anterior guardado)");
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
        var source = Exists(enabled) ? enabled : Exists(disabled) ? disabled : throw new ModException(ModProblem.NotFound, "Mod not found.");
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
        if (!Exists(from)) throw new ModException(ModProblem.NotFound, "Mod not found.");
        await JournaledOperation.RunAsync(journal, kind, port.Id, State(port, manifest, name), _ =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(to)!);
            if (Exists(to)) Backup(port, to);
            if (Directory.Exists(from)) Directory.Move(from, to);
            else File.Move(from, to);
            return Task.FromResult(new OperationStepResult(OperationOutcome.Success, State(port, manifest, name), name));
        }).ConfigureAwait(false);
    }

    private string? BackupExisting(Port port, string target, string disabled) =>
        Exists(target) ? Backup(port, target) : Exists(disabled) ? Backup(port, disabled) : null;

    private static bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

    private string Backup(Port port, string path)
    {
        var folder = Path.Combine(BackupRoot(port), time.GetUtcNow().ToString("yyyyMMdd-HHmmss-fff", System.Globalization.CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, Path.GetFileName(path));
        if (Directory.Exists(path)) Directory.Move(path, target);
        else File.Move(path, target);
        return target;
    }

    private static async Task CopyTreeAsync(string source, string target, CancellationToken cancellationToken)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await using var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true);
            await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true);
            await input.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
        }
    }

    private static void CheckSize(IEnumerable<long> sizes)
    {
        long total = 0;
        foreach (var size in sizes)
            if ((total += size) > MaxBytes) throw new ModException(ModProblem.TooLarge, "The mod is too large.");
    }

    private static long FolderSize(DirectoryInfo folder)
    {
        try { return folder.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return 0; }
    }

    private string Folder(Port port, InstalledPort manifest)
    {
        if (port.Mods is null) throw new ModException(ModProblem.NotOffered, "This port does not declare mod support.");
        return ModsFolder(port, manifest) ?? throw new ModException(ModProblem.NotInstalled, "Not installed.");
    }

    /// <summary>Only a plain name: no folders, no "..".</summary>
    private static string Safe(string name)
    {
        var file = Path.GetFileName(name);
        if (string.IsNullOrWhiteSpace(file) || file != name || file is "." or ".." || file.EndsWith(".lotusports-part", StringComparison.OrdinalIgnoreCase))
            throw new ModException(ModProblem.NotFound, "Invalid mod name.");
        return file;
    }

    private string State(Port port, InstalledPort manifest, string name) =>
        JsonSerializer.Serialize(new { mod = name, mods = List(port, manifest).Select(m => new { m.Name, State = m.State.ToString() }) }, StateJson);
}
