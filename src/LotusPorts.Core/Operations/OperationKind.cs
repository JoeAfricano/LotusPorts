namespace LotusPorts.Core.Operations;

/// <summary>Every kind of operation that changes disk or system. Saves and backups belong to Lotus, not here.</summary>
public enum OperationKind
{
    Install,
    Update,
    Rollback,
    Remove,
    CopyGameFile,
    ConvertGameFile,
    ApplyPreset,
    RevertPreset,
    DependencyInstall,
}

public enum OperationOutcome
{
    Success,
    Failure,
    Cancelled,
    Partial,
}
