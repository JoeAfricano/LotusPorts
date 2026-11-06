using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Verification;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Install;

public enum InstallStage
{
    Downloading,
    Extracting,
    Finishing,
}

/// <summary>Progress shown on the game's button: the stage and how far it is (0–1).</summary>
public sealed record InstallProgress(InstallStage Stage, double Fraction);

public enum InstallFailure
{
    /// <summary>The downloaded file is not the one GitHub (or the catalog) announced.</summary>
    IntegrityMismatch,

    /// <summary>The program to open could not be found in the version.</summary>
    NoExecutable,

    UnsupportedPackage,
    NotInstalled,
    NoPreviousVersion,
}

public sealed class InstallException(InstallFailure failure, string message) : Exception(message)
{
    public InstallFailure Failure { get; } = failure;
}

/// <summary>
/// Installs, updates, rolls back and removes ports (master prompt, section 7). Every change is journaled with the
/// state BEFORE and AFTER. Versions sit side by side; the active one and the one before it are kept, so an update can
/// always be undone. Files in the player's own folders (persistentPaths with location "user") are never touched.
/// </summary>
public sealed class PortInstaller(PortLibrary library, AppPaths paths, GitHubClient github, IOperationJournal journal, TimeProvider time, ILogger<PortInstaller> logger)
{
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    /// <summary>Downloads, checks and installs the chosen version, then makes it the active one.</summary>
    public async Task<InstalledPort> InstallAsync(Port port, ReleaseChoice choice, IProgress<InstallProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(choice);
        PortLibrary.EnsureId(port.Id);
        if (!ReleasePicker.CanInstall(port.WindowsAsset)) throw new InstallException(InstallFailure.UnsupportedPackage, "This package type is not installed by Lotus Ports.");

        var existing = library.Read(port.Id);
        InstalledPort? result = null;
        await JournaledOperation.RunAsync(journal, existing is null ? OperationKind.Install : OperationKind.Update, port.Id, State(existing), async ct =>
        {
            result = await InstallCoreAsync(port, choice, existing, progress, ct).ConfigureAwait(false);
            return new OperationStepResult(OperationOutcome.Success, State(result), $"{choice.Release.Tag} ({result.Active.Integrity})");
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result!;
    }

    private async Task<InstalledPort> InstallCoreAsync(Port port, ReleaseChoice choice, InstalledPort? existing, IProgress<InstallProgress>? progress, CancellationToken ct)
    {
        var tag = choice.Release.Tag;
        var folderName = VersionFolders.For(tag);
        var final = library.VersionFolder(port.Id, folderName);
        var staging = final + PortLibrary.StagingSuffix;
        var downloads = Path.Combine(paths.DownloadsCache, port.Id);
        var part = Path.Combine(downloads, VersionFolders.For(choice.Asset.Name) + ".part");

        InstalledVersion version;
        try
        {
            if (existing?.Versions.FirstOrDefault(v => v.Tag == tag) is { } kept && Directory.Exists(final))
            {
                version = kept; // already on disk: only made active again
            }
            else
            {
                Directory.CreateDirectory(downloads);
                var size = choice.Asset.Size;
                var sha = await github.DownloadAsync(choice.Asset, part,
                    new Progress<long>(done => progress?.Report(new(InstallStage.Downloading, size > 0 ? (double)done / size : 0))), ct).ConfigureAwait(false);
                var integrity = Verify(sha, choice.Asset, port.Evidence);
                if (integrity == CheckState.Failure)
                    throw new InstallException(InstallFailure.IntegrityMismatch, "The downloaded file does not match the one announced.");

                progress?.Report(new(InstallStage.Extracting, 0));
                PortLibrary.DeleteFolder(staging);
                if (port.WindowsAsset.PackageType == "zip")
                {
                    await Task.Run(() => SafeExtractor.ExtractZip(part, staging, progress: new Progress<double>(f => progress?.Report(new(InstallStage.Extracting, f))), cancellationToken: ct), ct).ConfigureAwait(false);
                }
                else
                {
                    Directory.CreateDirectory(staging);
                    File.Copy(part, Path.Combine(staging, Path.GetFileName(choice.Asset.Name)));
                }
                var exe = ExecutableFinder.Find(staging, port.Executable)
                    ?? throw new InstallException(InstallFailure.NoExecutable, "The program to open was not found in this version.");

                progress?.Report(new(InstallStage.Finishing, 0));
                PortLibrary.DeleteFolder(final); // a leftover from an unfinished install, not listed in active.json
                Directory.Move(staging, final);
                version = new InstalledVersion(tag, folderName, choice.Asset.Name, choice.Asset.Size, sha, integrity, exe, time.GetUtcNow());
            }
        }
        finally
        {
            TryDelete(part);
            if (Directory.Exists(staging)) TryDeleteFolder(staging);
        }

        // Saves and settings that live inside the game folder move along to the new version (the old copy stays as it was).
        var keptFolder = library.KeptFolder(port.Id);
        var source = existing is not null && existing.ActiveTag != tag ? library.VersionFolder(port.Id, existing.Active.Folder)
            : existing is null && Directory.Exists(keptFolder) ? keptFolder : null;
        if (source is not null) CopyPersistent(port, source, final);
        if (existing is null && Directory.Exists(keptFolder)) TryDeleteFolder(keptFolder);

        var previous = existing is null ? null : existing.ActiveTag == tag ? existing.PreviousTag : existing.ActiveTag;
        var keep = new HashSet<string?> { tag, previous, existing?.PinnedTag };
        var versions = (existing?.Versions ?? []).Where(v => v.Tag != tag).Append(version).ToList();
        foreach (var old in versions.Where(v => !keep.Contains(v.Tag)).ToList())
        {
            TryDeleteFolder(library.VersionFolder(port.Id, old.Folder));
            versions.Remove(old);
        }

        var manifest = new InstalledPort(InstalledPort.CurrentSchema, port.Id, tag, previous, versions,
            PendingFirstLaunch: existing is not null && existing.ActiveTag != tag, LastLaunchFailed: false, existing?.PinnedTag);
        library.Write(manifest);
        progress?.Report(new(InstallStage.Finishing, 1));
        return manifest;
    }

    /// <summary>"Voltar para a versão anterior": the previous version becomes active again, with its own saves as they were.</summary>
    public async Task<InstalledPort> RollbackAsync(Port port, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var existing = library.Read(port.Id) ?? throw new InstallException(InstallFailure.NotInstalled, "Not installed.");
        var previous = existing.Previous;
        if (previous is null || !Directory.Exists(library.VersionFolder(port.Id, previous.Folder)))
            throw new InstallException(InstallFailure.NoPreviousVersion, "There is no previous version on this PC.");

        InstalledPort? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.Rollback, port.Id, State(existing), _ =>
        {
            result = existing with { ActiveTag = previous.Tag, PreviousTag = existing.ActiveTag, PendingFirstLaunch = false, LastLaunchFailed = false };
            library.Write(result);
            return Task.FromResult(new OperationStepResult(OperationOutcome.Success, State(result), $"{existing.ActiveTag} -> {previous.Tag}"));
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result!;
    }

    /// <summary>
    /// Uninstalls. With <paramref name="keepSaves"/>, saves and settings inside the game folder are kept for the next
    /// install. Files in the player's own folders (outside the library) are never removed here.
    /// </summary>
    public async Task RemoveAsync(Port port, bool keepSaves, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var existing = library.Read(port.Id) ?? throw new InstallException(InstallFailure.NotInstalled, "Not installed.");
        await JournaledOperation.RunAsync(journal, OperationKind.Remove, port.Id, State(existing), _ =>
        {
            var folder = library.PortFolder(port.Id);
            if (keepSaves)
            {
                var kept = library.KeptFolder(port.Id);
                CopyPersistent(port, library.VersionFolder(port.Id, existing.Active.Folder), kept);
                library.DeleteManifest(port.Id);
                PortLibrary.DeleteFolder(library.VersionsFolder(port.Id));
            }
            else
            {
                library.DeleteManifest(port.Id);
                PortLibrary.DeleteFolder(folder);
            }
            var after = JsonSerializer.Serialize(new { installed = false, keptSaves = keepSaves }, StateJson);
            return Task.FromResult(new OperationStepResult(OperationOutcome.Success, after, keepSaves ? "kept saves" : "removed all"));
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>How the first launch after an update went: a quick crash offers "Voltar para a versão anterior".</summary>
    public InstalledPort? RecordFirstLaunch(string portId, bool failed)
    {
        if (library.Read(portId) is not { PendingFirstLaunch: true } manifest) return library.Read(portId);
        var updated = manifest with { PendingFirstLaunch = false, LastLaunchFailed = failed };
        library.Write(updated);
        return updated;
    }

    /// <summary>The problem was looked at (the player chose to keep this version).</summary>
    public InstalledPort? ClearProblem(string portId)
    {
        if (library.Read(portId) is not { LastLaunchFailed: true } manifest) return library.Read(portId);
        var updated = manifest with { LastLaunchFailed = false };
        library.Write(updated);
        return updated;
    }

    /// <summary>
    /// OK only when the hash matches the one GitHub publishes for the file (or the one the catalog recorded for this
    /// exact file); FAILURE when it differs; NOT VERIFIED when nobody published one.
    /// </summary>
    public static CheckState Verify(string sha256, ReleaseAssetInfo asset, PortEvidence? evidence)
    {
        ArgumentNullException.ThrowIfNull(asset);
        var expected = asset.Sha256
            ?? (evidence is { AssetSha256: { } recorded, AssetName: { } name } && name == asset.Name && evidence.AssetSize == asset.Size ? recorded : null);
        if (expected is null) return CheckState.NotVerified;
        return string.Equals(expected, sha256, StringComparison.OrdinalIgnoreCase) ? CheckState.Ok : CheckState.Failure;
    }

    /// <summary>Copies the port's in-folder saves/settings (persistentPaths with location "install") from one folder to another.</summary>
    public static int CopyPersistent(Port port, string from, string to)
    {
        ArgumentNullException.ThrowIfNull(port);
        var copied = 0;
        var fromRoot = Path.GetFullPath(from).TrimEnd('\\') + "\\";
        var toRoot = Path.GetFullPath(to).TrimEnd('\\') + "\\";
        foreach (var entry in port.PersistentPaths.Where(p => p.Location == "install"))
        {
            var relative = entry.Path.Replace('/', '\\').TrimEnd('\\');
            if (relative.Length == 0 || Path.IsPathRooted(relative) || relative.Split('\\').Contains("..")) continue;
            var source = Path.GetFullPath(Path.Combine(fromRoot, relative));
            var target = Path.GetFullPath(Path.Combine(toRoot, relative));
            if (!source.StartsWith(fromRoot, StringComparison.OrdinalIgnoreCase) || !target.StartsWith(toRoot, StringComparison.OrdinalIgnoreCase)) continue;
            if (File.Exists(source))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target, overwrite: true);
                copied++;
            }
            else if (Directory.Exists(source) && new DirectoryInfo(source).LinkTarget is null)
            {
                foreach (var file in Directory.EnumerateFiles(source, "*", new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = FileAttributes.ReparsePoint }))
                {
                    var dest = Path.Combine(target, Path.GetRelativePath(source, file));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    File.Copy(file, dest, overwrite: true);
                    copied++;
                }
            }
        }
        return copied;
    }

    private static string State(InstalledPort? m) => m is null
        ? JsonSerializer.Serialize(new { installed = false }, StateJson)
        : JsonSerializer.Serialize(new
        {
            installed = true,
            activeTag = m.ActiveTag,
            previousTag = m.PreviousTag,
            versions = m.Versions.Select(v => v.Tag),
            integrity = m.Active.Integrity.ToString(),
            m.PendingFirstLaunch,
            m.LastLaunchFailed,
        }, StateJson);

    private void TryDelete(string file)
    {
        try { if (File.Exists(file)) File.Delete(file); }
        catch (IOException ex) { logger.LogDebug(ex, "Could not delete a partial download"); }
    }

    private void TryDeleteFolder(string folder)
    {
        try { PortLibrary.DeleteFolder(folder); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { logger.LogWarning(ex, "Could not delete an old folder"); }
    }
}
