using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.GameFiles;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;

namespace LotusPorts.Infrastructure.Install;

/// <summary>
/// For ports that read the game file from their own folder (setup "beside-exe"): copies the player's recognized copy to
/// the place and name the port expects, so the game opens ready. The player's file is only read; an N64 dump is written
/// in .z64 order; a different file already there is kept in a backup, never overwritten; the copy is journaled.
/// </summary>
public sealed class GameFilePlacer(PortLibrary library, IOperationJournal journal, TimeProvider time)
{
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    /// <summary>The port reads one game file from its folder (not a set of files the player arranges).</summary>
    public static bool Offers(Port port) =>
        port is not null && port.Setup.Kind == "beside-exe" && port.RequiredGameFiles.Count == 1
        && port.RequiredGameFiles[0].Kind is "single-file" or "disc-image";

    /// <summary>Where the game file goes for the active version (inside the game folder only).</summary>
    public string TargetPath(Port port, InstalledPort manifest, string sourcePath)
    {
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(manifest);
        var root = Path.GetFullPath(library.VersionFolder(port.Id, manifest.Active.Folder)).TrimEnd('\\') + "\\";
        var folder = port.Setup.TargetFolder?.Replace('/', '\\').Trim('\\') ?? "";
        var name = Path.GetFileName(port.Setup.TargetFileName ?? Path.GetFileName(sourcePath));
        var target = Path.GetFullPath(Path.Combine(root, folder, name));
        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase) || folder.Split('\\').Contains(".."))
            throw new InvalidOperationException("The game file must go inside the game folder.");
        return target;
    }

    /// <summary>The game file is already in place (same size as the player's copy).</summary>
    public bool IsPlaced(Port port, InstalledPort manifest, string sourcePath)
    {
        var target = TargetPath(port, manifest, sourcePath);
        return File.Exists(target) && File.Exists(sourcePath) && new FileInfo(target).Length == new FileInfo(sourcePath).Length;
    }

    public async Task<string> PlaceAsync(Port port, InstalledPort manifest, GameFileInfo file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var target = TargetPath(port, manifest, file.Path);
        string State(string? backup) => JsonSerializer.Serialize(new { file = Path.GetFileName(file.Path), target, exists = File.Exists(target), backup }, StateJson);
        await JournaledOperation.RunAsync(journal, OperationKind.CopyGameFile, port.Id, State(null), async ct =>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            string? backup = null;
            if (File.Exists(target))
            {
                var folder = Path.Combine(library.PortFolder(port.Id), "game-file-backups", time.GetUtcNow().ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture));
                Directory.CreateDirectory(folder);
                backup = Path.Combine(folder, Path.GetFileName(target));
                File.Move(target, backup);
            }
            var temp = target + ".lotusports-part";
            if (File.Exists(temp)) File.Delete(temp);
            if (file.Format == GameFileFormat.N64)
            {
                await GameFileReader.WriteCanonicalCopyAsync(file.Path, temp, ct).ConfigureAwait(false);
            }
            else
            {
                await using var input = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16, useAsync: true);
                await using var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true);
                await input.CopyToAsync(output, ct).ConfigureAwait(false);
            }
            File.Move(temp, target);
            return new OperationStepResult(OperationOutcome.Success, State(backup), Path.GetFileName(target));
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return target;
    }
}
