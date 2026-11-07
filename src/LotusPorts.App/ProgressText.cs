using LotusPorts.Infrastructure.Install;

namespace LotusPorts.App;

/// <summary>One wording for an install in progress, wherever it is shown (card, game page, Downloads).</summary>
public static class ProgressText
{
    public static string For(InstallProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        var percent = (int)Math.Round(progress.Fraction * 100);
        return progress.Stage switch
        {
            InstallStage.Queued => Strings.Get("PlayQueued"),
            InstallStage.Downloading => Strings.Format("PlayDownloading", percent),
            InstallStage.Extracting => Strings.Format("PlayExtracting", percent),
            _ => Strings.Get("PlayFinishing"),
        };
    }

    /// <summary>The bar has no percentage to show while waiting or finishing.</summary>
    public static bool Indeterminate(InstallProgress progress) => progress.Stage is InstallStage.Finishing or InstallStage.Queued;
}
