namespace LotusPorts.Infrastructure.Database.Migrations;

/// <summary>Phase 0: preferences and the BEFORE/AFTER operation journal. Other tables arrive with the phases that use them.</summary>
public sealed class M001Foundation : IMigration
{
    public int Version => 1;
    public string Name => "Foundation";
    public string Sql => """
        CREATE TABLE Settings (
            Key TEXT PRIMARY KEY NOT NULL,
            Value TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );
        CREATE TABLE OperationJournal (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            StartedAt TEXT NOT NULL,
            FinishedAt TEXT NULL,
            Kind TEXT NOT NULL,
            PortId TEXT NULL,
            StateBefore TEXT NOT NULL,
            StateAfter TEXT NULL,
            Outcome TEXT NULL,
            Details TEXT NULL,
            CorrelationId TEXT NOT NULL
        );
        CREATE INDEX IX_OperationJournal_StartedAt ON OperationJournal (StartedAt);
        CREATE INDEX IX_OperationJournal_Open ON OperationJournal (Outcome) WHERE Outcome IS NULL;
        """;
}

public static class AllMigrations
{
    public static IReadOnlyList<IMigration> List { get; } = [new M001Foundation()];
}
