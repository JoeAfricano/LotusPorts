namespace LotusPorts.Core.Install;

/// <summary>What the main button on a game shows (master prompt, section 7.3).</summary>
public enum PlayButtonState
{
    /// <summary>Port still being built: "Acompanhar" opens the official page.</summary>
    Follow,

    /// <summary>The game file is missing: "Adicionar arquivo do jogo".</summary>
    AddGameFile,

    Install,

    /// <summary>Downloading or installing: progress bar and Cancel.</summary>
    Working,

    Play,

    /// <summary>Update available and automatic updates on.</summary>
    UpdateAndPlay,

    /// <summary>Problem detected: "Resolver" with the cause.</summary>
    Resolve,

    /// <summary>The game is open: "Jogando…".</summary>
    Running,

    /// <summary>Lotus Ports cannot install this port by itself (own installer, unknown package): opens the official page.</summary>
    OfficialPage,
}

/// <summary>Second actions next to the main button or in the ⋯ menu.</summary>
public enum SecondaryAction
{
    OtherWays,
    AcceptedVersions,
    Cancel,
    PlayWithoutUpdate,
    Update,
    Rollback,
    Details,
    CloseGame,
    OpenFolder,
    Uninstall,
}

/// <summary>Everything the button depends on. Each flag is a fact already checked, never a guess.</summary>
public sealed record PlayContext(
    bool StillBeingBuilt,
    bool CanInstall,
    bool Installed,
    bool Working,
    bool Running,
    bool UpdateAvailable,
    bool AutoUpdate,
    bool ProblemDetected,
    bool CanRollback,
    bool GameFileMissing = false);

public sealed record PlayButton(PlayButtonState State, IReadOnlyList<SecondaryAction> Secondary);

public static class PlayButtonRules
{
    public static PlayButton Decide(PlayContext c)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Working) return new(PlayButtonState.Working, [SecondaryAction.Cancel]);
        if (c.Running) return new(PlayButtonState.Running, [SecondaryAction.CloseGame]);
        if (!c.Installed)
        {
            if (c.StillBeingBuilt) return new(PlayButtonState.Follow, []);
            return c.CanInstall
                ? new(PlayButtonState.Install, [SecondaryAction.OtherWays])
                : new(PlayButtonState.OfficialPage, [SecondaryAction.OtherWays]);
        }

        List<SecondaryAction> manage = [SecondaryAction.OpenFolder, SecondaryAction.Uninstall];
        if (c.ProblemDetected)
            return new(PlayButtonState.Resolve, c.CanRollback ? [SecondaryAction.Rollback, SecondaryAction.Details, .. manage] : [SecondaryAction.Details, .. manage]);
        if (c.GameFileMissing) return new(PlayButtonState.AddGameFile, [SecondaryAction.AcceptedVersions, .. manage]);
        if (c.UpdateAvailable)
            return c.AutoUpdate
                ? new(PlayButtonState.UpdateAndPlay, [SecondaryAction.PlayWithoutUpdate, .. manage])
                : new(PlayButtonState.Play, [SecondaryAction.Update, .. manage]);
        return new(PlayButtonState.Play, c.CanRollback ? [SecondaryAction.Rollback, .. manage] : manage);
    }
}
