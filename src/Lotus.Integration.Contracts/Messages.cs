namespace Lotus.Integration.Contracts;

/// <summary>
/// Parameters and results of each method (master prompt, section 12). Records only: both apps serialize them with
/// <see cref="ContractJson.Options"/>. A capability is the method or event name the side announces in the handshake.
/// </summary>
public sealed record DependenciesCheckParams(IReadOnlyList<string> Ids);

public sealed record DependencyState(string Id, CheckStatus State, string? InstalledVersion, string? Details);

public sealed record DependenciesCheckResult(IReadOnlyList<DependencyState> Items);

public sealed record DependenciesRequestInstallParams(IReadOnlyList<string> Ids, string? Reason);

public sealed record SystemChecksParams(IReadOnlyList<string> GraphicsApis, string? ExePath);

/// <summary>One system check from Lotus, in the same OK / AVISO / FALHA / NÃO VERIFICADO scale. Texts come in pt-BR.</summary>
public sealed record SystemCheckItem(string Id, CheckStatus State, string TitlePtBr, string DetailsPtBr);

public sealed record SystemChecksResult(IReadOnlyList<SystemCheckItem> Items);

public sealed record JournalQueryRecentParams(int Days);

/// <summary>A change Lotus recorded on the PC (driver update, setting applied…), shown as a clue in "Resolver".</summary>
public sealed record SystemChange(DateTimeOffset At, string Kind, string DescriptionPtBr);

public sealed record JournalQueryRecentResult(IReadOnlyList<SystemChange> Changes);

public sealed record SessionStartParams(string LotusGameKey, string GameTitle, string PortId, string? PortVersion, string ExePath, int ProcessId, string? Preset);

public sealed record SessionStartResult(string SessionId);

public sealed record SessionEndParams(string SessionId, int ExitCode, double DurationSeconds);

/// <summary>Body of the events; each event uses only the fields it needs.</summary>
public sealed record GameEvent(string GameId, string PortId, string? Version);

public sealed record DriverChangedEvent(string Device, string? Version, DateTimeOffset At);
