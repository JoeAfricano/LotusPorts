using System.ComponentModel;
using System.Diagnostics;
using LotusPorts.Catalog;
using LotusPorts.Core.Install;

namespace LotusPorts.Infrastructure.Install;

public enum LaunchProblem
{
    None,
    MissingProgram,

    /// <summary>The program asks for administrator rights; Lotus Ports runs without them and does not elevate.</summary>
    NeedsAdministrator,

    CouldNotStart,
}

public sealed record LaunchResult(Process? Process, LaunchProblem Problem, string? Details = null);

/// <summary>Opens the active version of a port, from its own folder, with the catalog's arguments. Never elevated.</summary>
public sealed class GameLauncher(PortLibrary library)
{
    private const int ErrorElevationRequired = 740;

    /// <summary>A game that closes with an error before this is treated as "did not open".</summary>
    public static readonly TimeSpan QuickExit = TimeSpan.FromSeconds(15);

    public string ProgramPath(Port port, InstalledPort manifest)
    {
        ArgumentNullException.ThrowIfNull(port);
        ArgumentNullException.ThrowIfNull(manifest);
        return Path.Combine(library.VersionFolder(port.Id, manifest.Active.Folder), manifest.Active.Executable);
    }

    public LaunchResult Start(Port port, InstalledPort manifest)
    {
        var exe = ProgramPath(port, manifest);
        if (!File.Exists(exe)) return new(null, LaunchProblem.MissingProgram);
        var info = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(exe)!,
        };
        foreach (var arg in port.LaunchArgs ?? []) info.ArgumentList.Add(arg);
        try
        {
            var process = Process.Start(info);
            if (process is null) return new(null, LaunchProblem.CouldNotStart);
            process.EnableRaisingEvents = true;
            return new(process, LaunchProblem.None);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorElevationRequired)
        {
            return new(null, LaunchProblem.NeedsAdministrator);
        }
        catch (Win32Exception ex)
        {
            return new(null, LaunchProblem.CouldNotStart, ex.Message);
        }
    }

    /// <summary>"Did not open": closed with an error code within <see cref="QuickExit"/>.</summary>
    public static bool FailedQuickly(TimeSpan ranFor, int exitCode) => ranFor < QuickExit && exitCode != 0;
}
