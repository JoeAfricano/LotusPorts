namespace LotusPorts.Infrastructure.Database.Migrations;

/// <summary>Phase 1: per-game player state and imported catalog sources.</summary>
public sealed class M002Catalog : IMigration
{
    public int Version => 2;
    public string Name => "Catalog";
    public string Sql => """
        CREATE TABLE Games (
            GameId TEXT PRIMARY KEY NOT NULL,
            FirstSeenAt TEXT NOT NULL,
            Favorite INTEGER NOT NULL DEFAULT 0,
            DismissedFromNew INTEGER NOT NULL DEFAULT 0,
            Followed INTEGER NOT NULL DEFAULT 0
        );
        CREATE TABLE CatalogSources (
            Id TEXT PRIMARY KEY NOT NULL,
            Url TEXT NOT NULL UNIQUE,
            Name TEXT NOT NULL,
            Active INTEGER NOT NULL DEFAULT 1,
            Trusted INTEGER NOT NULL DEFAULT 0,
            ETag TEXT NULL,
            LastUpdatedAt TEXT NULL,
            ValidationState TEXT NOT NULL,
            ValidationMessage TEXT NULL,
            AddedAt TEXT NOT NULL
        );
        """;
}
