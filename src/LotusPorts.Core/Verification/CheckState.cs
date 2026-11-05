namespace LotusPorts.Core.Verification;

/// <summary>Result of any check. Only <see cref="Ok"/> may be shown as success, and only after the check really ran.</summary>
public enum CheckState
{
    NotVerified = 0,
    Ok,
    Warning,
    Failure,
}
