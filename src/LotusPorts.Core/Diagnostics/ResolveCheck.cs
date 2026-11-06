using LotusPorts.Core.Verification;

namespace LotusPorts.Core.Diagnostics;

/// <summary>The "Resolver" checks (master prompt, section 10.2), in the order they are shown.</summary>
public enum ResolveCheckKind
{
    Program,
    GameFile,
    Dependencies,
    Graphics,
    Path,
    DiskSpace,
    LastCrash,
    Saves,
    RecentUpdate,
}

/// <summary>What the player can do about a check, shown as a button.</summary>
public enum ResolveAction
{
    None,
    Rollback,
    Reinstall,
    AddGameFile,
    OpenFolder,
}

/// <summary>
/// One check: its state, the reason (a key the app turns into plain words) and facts that fill the text (a path, a code).
/// <see cref="CheckState.Ok"/> only when the check really ran and passed; otherwise Warning, Failure or NotVerified.
/// </summary>
public sealed record ResolveCheck(ResolveCheckKind Kind, CheckState State, string Reason, IReadOnlyList<string> Facts, ResolveAction Action = ResolveAction.None)
{
    public bool IsProblem => State is CheckState.Warning or CheckState.Failure;

    public static ResolveCheck Ok(ResolveCheckKind kind, string reason, params string[] facts) => new(kind, CheckState.Ok, reason, facts);

    public static ResolveCheck NotVerified(ResolveCheckKind kind, string reason, params string[] facts) => new(kind, CheckState.NotVerified, reason, facts);
}

/// <summary>A crash recorded by Windows for the game's program (Application Error, event 1000).</summary>
public sealed record CrashRecord(DateTimeOffset At, string Program, string FaultingModule, string ExceptionCode);

public sealed record ResolveReport(string Game, string Port, string? Version, DateTimeOffset At, IReadOnlyList<ResolveCheck> Checks)
{
    /// <summary>The most important problem first: failures, then warnings, in check order.</summary>
    public ResolveCheck? MainProblem => Checks.Where(c => c.State == CheckState.Failure).Concat(Checks.Where(c => c.State == CheckState.Warning)).FirstOrDefault();
}
