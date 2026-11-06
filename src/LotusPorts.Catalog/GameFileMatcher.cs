using LotusPorts.Core.GameFiles;

namespace LotusPorts.Catalog;

/// <summary>A file recognized as an accepted version of a game for a port (a hash matched the catalog).</summary>
public sealed record GameFileMatch(LibraryGame Game, Port Port, RequiredGameFile File, AcceptedVersion Version, string MatchedBy)
{
    /// <summary>"EUA 1.0", or only the region when the version was not filled in.</summary>
    public string VersionLabel => Version.Version.StartsWith("PREENCHER", StringComparison.Ordinal) ? Version.Region : $"{Version.Region} {Version.Version}";
}

/// <summary>
/// Compares a file's hashes with the accepted versions in the catalog. Only an exact hash match recognizes a file;
/// placeholders ("PREENCHER…") never match. Nothing leaves the PC: the comparison is local.
/// </summary>
public static class GameFileMatcher
{
    public static IReadOnlyList<GameFileMatch> Match(IEnumerable<LibraryGame> games, GameFileHashes hashes)
    {
        ArgumentNullException.ThrowIfNull(games);
        ArgumentNullException.ThrowIfNull(hashes);
        var matches = new List<GameFileMatch>();
        foreach (var game in games)
            foreach (var choice in game.AllPorts)
                foreach (var file in choice.Port.RequiredGameFiles)
                    foreach (var version in file.AcceptedVersions)
                        if (MatchedBy(version, hashes) is { } by) matches.Add(new GameFileMatch(game, choice.Port, file, version, by));
        return matches;
    }

    /// <summary>The strongest hash that matched (SHA-256, SHA-1, MD5, then CRC32), or null.</summary>
    public static string? MatchedBy(AcceptedVersion version, GameFileHashes hashes)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(hashes);
        if (Same(version.Sha256, hashes.Sha256, 64)) return "SHA-256";
        if (Same(version.Sha1, hashes.Sha1, 40)) return "SHA-1";
        if (Same(version.Md5, hashes.Md5, 32)) return "MD5";
        // CRC32 alone is weak; it only counts when no stronger hash was recorded for this version.
        if (version.Sha256 is null && version.Sha1 is null && version.Md5 is null && Same(version.Crc32, hashes.Crc32, 8)) return "CRC32";
        return null;
    }

    private static bool Same(string? expected, string actual, int length) =>
        expected is { } e && e.Length == length && e.All(Uri.IsHexDigit) && string.Equals(e, actual, StringComparison.OrdinalIgnoreCase);

    /// <summary>The accepted versions of a port, for "Este jogo aceita: …".</summary>
    public static IReadOnlyList<string> AcceptedLabels(Port port)
    {
        ArgumentNullException.ThrowIfNull(port);
        return port.RequiredGameFiles.SelectMany(f => f.AcceptedVersions)
            .Select(v => v.Version.StartsWith("PREENCHER", StringComparison.Ordinal) ? v.Region : $"{v.Region} {v.Version}")
            .Distinct().ToList();
    }

    /// <summary>The game file formats a platform uses, to tell "wrong console" from "wrong version".</summary>
    public static bool FitsPlatform(GameFileFormat format, Platform platform) => (format, platform) switch
    {
        (GameFileFormat.N64, Platform.N64) => true,
        (GameFileFormat.Snes, Platform.SNES) => true,
        (GameFileFormat.Gba, Platform.GBA) => true,
        (GameFileFormat.Nds, Platform.NDS) => true,
        (GameFileFormat.Xbox360Executable, Platform.XBOX_360) => true,
        (GameFileFormat.WiiUDump, Platform.WII_U) => true,
        (GameFileFormat.DiscImage, Platform.PS1 or Platform.PS2 or Platform.PS3 or Platform.GAMECUBE or Platform.WII or Platform.XBOX or Platform.XBOX_360
            or Platform.SATURN or Platform.DREAMCAST or Platform.PSP or Platform.CDI) => true,
        _ => false,
    };
}
