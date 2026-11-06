using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;
using LotusPorts.Core.Operations;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Styles;

namespace LotusPorts.Infrastructure.Install;

public enum StyleProblem
{
    None,

    /// <summary>The port's style keys were not confirmed in its real configuration: the picker does not appear.</summary>
    NotVerified,

    NotInstalled,
    UnsupportedFormat,

    /// <summary>The configuration path in the catalog points outside the game folder.</summary>
    InvalidPath,

    /// <summary>The file read back did not have the values: the backup was put back.</summary>
    NotConfirmed,
}

public sealed class StyleException(StyleProblem problem, string message) : Exception(message)
{
    public StyleProblem Problem { get; } = problem;
}

/// <summary>
/// Applies Original or Remasterizado to the active version (master prompt, section 5.6): backup of the configuration
/// file, writing only the declared keys, reading back to confirm, journal BEFORE/AFTER. Reverting puts back only those
/// keys, so other settings the player changed meanwhile stay.
/// </summary>
public sealed class StyleService(PortLibrary library, IOperationJournal journal, TimeProvider time)
{
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    public static bool CanOffer(Port port) =>
        port is not null && port.ExperiencePresets.IsVerified && PresetWriter.Supports(port.ExperiencePresets.ConfigFormat);

    public async Task<InstalledPort> ApplyAsync(Port port, ExperienceStyle style, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        if (!CanOffer(port)) throw new StyleException(port.ExperiencePresets.IsVerified ? StyleProblem.UnsupportedFormat : StyleProblem.NotVerified, "Style not available.");
        var manifest = library.Read(port.Id) ?? throw new StyleException(StyleProblem.NotInstalled, "Not installed.");
        if (manifest.Style is { } current && current.Tag == manifest.ActiveTag) manifest = await RevertAsync(port, cancellationToken).ConfigureAwait(false);

        var presets = port.ExperiencePresets;
        var values = (style == ExperienceStyle.Remastered ? presets.Remasterizado : presets.Original).Values;
        var file = ConfigPath(port, manifest);
        var format = presets.ConfigFormat!;
        InstalledPort? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.ApplyPreset, port.Id, State(manifest, file), async ct =>
        {
            var before = File.Exists(file) ? await File.ReadAllTextAsync(file, ct).ConfigureAwait(false) : "";
            string? backup = null;
            if (File.Exists(file))
            {
                var folder = Path.Combine(library.PortFolder(port.Id), "style-backups");
                Directory.CreateDirectory(folder);
                backup = Path.Combine(folder, $"{time.GetUtcNow():yyyyMMddHHmmss}-{Path.GetFileName(file)}");
                File.Copy(file, backup, overwrite: true);
            }
            var (text, previous) = PresetWriter.Apply(before, format, values);
            await WriteAsync(file, text, ct).ConfigureAwait(false);
            if (!PresetWriter.Verify(await File.ReadAllTextAsync(file, ct).ConfigureAwait(false), format, values))
            {
                await WriteAsync(file, before, ct).ConfigureAwait(false);
                throw new StyleException(StyleProblem.NotConfirmed, "The configuration did not keep the style values.");
            }
            result = manifest with { Style = new AppliedStyle(style.ToString(), manifest.ActiveTag, previous, backup, time.GetUtcNow()) };
            library.Write(result);
            return new OperationStepResult(OperationOutcome.Success, State(result, file), style.ToString());
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result!;
    }

    public async Task<InstalledPort> RevertAsync(Port port, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(port);
        var manifest = library.Read(port.Id) ?? throw new StyleException(StyleProblem.NotInstalled, "Not installed.");
        if (manifest.Style is not { } applied) return manifest;
        var file = ConfigPath(port, manifest);
        var format = port.ExperiencePresets.ConfigFormat!;
        InstalledPort? result = null;
        await JournaledOperation.RunAsync(journal, OperationKind.RevertPreset, port.Id, State(manifest, file), async ct =>
        {
            // A style applied to another version is only forgotten: that version's file is not this one.
            if (applied.Tag == manifest.ActiveTag && File.Exists(file))
            {
                var text = PresetWriter.Revert(await File.ReadAllTextAsync(file, ct).ConfigureAwait(false), format, applied.Previous);
                await WriteAsync(file, text, ct).ConfigureAwait(false);
                var back = PresetWriter.Read(await File.ReadAllTextAsync(file, ct).ConfigureAwait(false), format, applied.Previous.Keys);
                if (applied.Previous.Any(kv => !SameValue(back[kv.Key], kv.Value, format)))
                    throw new StyleException(StyleProblem.NotConfirmed, "The previous values could not be confirmed.");
            }
            result = manifest with { Style = null };
            library.Write(result);
            return new OperationStepResult(OperationOutcome.Success, State(result, file), applied.Name);
        }, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result!;
    }

    private static bool SameValue(string? actual, string? expected, string format) =>
        actual == expected || (format == "json" && actual is not null && expected is not null
            && System.Text.Json.Nodes.JsonNode.DeepEquals(System.Text.Json.Nodes.JsonNode.Parse(actual), System.Text.Json.Nodes.JsonNode.Parse(expected)));

    private string ConfigPath(Port port, InstalledPort manifest)
    {
        var root = Path.GetFullPath(library.VersionFolder(port.Id, manifest.Active.Folder)).TrimEnd('\\') + "\\";
        var relative = port.ExperiencePresets.ConfigFile!.Replace('/', '\\');
        var path = Path.GetFullPath(Path.Combine(root, relative));
        if (Path.IsPathRooted(relative) || !path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new StyleException(StyleProblem.InvalidPath, "The configuration file must be inside the game folder.");
        return path;
    }

    private static async Task WriteAsync(string file, string text, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var temp = file + ".lotusports-tmp";
        await File.WriteAllTextAsync(temp, text, ct).ConfigureAwait(false);
        File.Move(temp, file, overwrite: true);
    }

    private static string State(InstalledPort m, string file) =>
        JsonSerializer.Serialize(new { activeTag = m.ActiveTag, style = m.Style?.Name, configFile = Path.GetFileName(file), configExists = File.Exists(file) }, StateJson);
}
