namespace LotusPorts.Infrastructure.Database.Migrations;

/// <summary>Phase 3: game files read on this PC (path + size + date as the cache key). Never leaves the PC.</summary>
public sealed class M003GameFiles : IMigration
{
    public int Version => 3;
    public string Name => "GameFiles";
    public string Sql => """
        CREATE TABLE GameFiles (
            Path TEXT PRIMARY KEY NOT NULL COLLATE NOCASE,
            Size INTEGER NOT NULL,
            ModifiedUtc TEXT NOT NULL,
            Format TEXT NOT NULL,
            ByteOrder TEXT NULL,
            InternalName TEXT NULL,
            GameCode TEXT NULL,
            RomVersion INTEGER NULL,
            Sha1 TEXT NULL,
            Md5 TEXT NULL,
            Sha256 TEXT NULL,
            Crc32 TEXT NULL,
            ReadAt TEXT NOT NULL
        );
        """;
}
