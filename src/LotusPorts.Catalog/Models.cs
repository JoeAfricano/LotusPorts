using System.Text.Json;
using System.Text.Json.Serialization;

namespace LotusPorts.Catalog;

// Mirrors catalog/catalog.schema.json. The schema is the source of truth; these types only read what it allows.

public sealed record CatalogBundle(int SchemaVersion, string Name, string Version, IReadOnlyList<Game> Games, IReadOnlyList<Port> Ports);

public sealed record Game(
    int SchemaVersion,
    string Id,
    string Title,
    Platform OriginalPlatform,
    int? ReleaseYear,
    IReadOnlyList<string> Genre,
    string DescriptionPtBr,
    string RecommendedPortId,
    string? RecommendationReasonPtBr,
    IReadOnlyList<PortAlternative> Alternatives,
    string LotusGameKey,
    Review Review,
    GameCoverArt? Cover = null);

/// <summary>Box art address (libretro-thumbnails). The app fetches and caches it when shown.</summary>
public sealed record GameCoverArt(string Url, string Source);

public sealed record PortAlternative(string PortId, string? DifferencePtBr);

public sealed record Review(ReviewState State, string? By, string? At, string? NotesPtBr);

[JsonConverter(typeof(JsonStringEnumConverter<ReviewState>))]
public enum ReviewState
{
    [JsonStringEnumMemberName("pending")] Pending,
    [JsonStringEnumMemberName("approved")] Approved,
    [JsonStringEnumMemberName("rejected")] Rejected,
}

[JsonConverter(typeof(JsonStringEnumConverter<Platform>))]
public enum Platform
{
    N64,
    SNES,
    NES,
    GB,
    GBC,
    GBA,
    NDS,
    N3DS,
    VIRTUAL_BOY,
    GAMECUBE,
    WII,
    WII_U,
    SWITCH,
    SWITCH_2,
    PS1,
    PS2,
    PS3,
    PS4,
    PS5,
    PSP,
    XBOX,
    XBOX_360,
    GENESIS,
    SEGA_32X,
    GAME_GEAR,
    SATURN,
    DREAMCAST,
    ARCADE,
    CDI,
    AMIGA,
    PC,
    MOBILE,
    OTHER,
}

public sealed record Port(
    int SchemaVersion,
    string Id,
    string GameId,
    string Title,
    string PortType,
    PortStatus Status,
    IReadOnlyList<string>? KnownIssuesPtBr,
    string? VerifiedBy,
    string? VerifiedAt,
    RepoRef Repo,
    string? License,
    WindowsAsset WindowsAsset,
    string? Executable,
    IReadOnlyList<string>? LaunchArgs,
    PortSetup Setup,
    IReadOnlyList<RequiredGameFile> RequiredGameFiles,
    IReadOnlyList<PersistentPath> PersistentPaths,
    ExperiencePresets ExperiencePresets,
    IReadOnlyList<string>? Dependencies,
    IReadOnlyList<string>? GraphicsApis,
    string? ControllerSupport,
    IReadOnlyList<string>? HighlightsPtBr,
    PortLinks Links,
    PortArt? Art,
    int? MinWindowsBuild,
    PortEvidence? Evidence,
    PortMods? Mods = null);

/// <summary>
/// How the port officially takes mods (Phase 8, experimental): the folder the game reads, the accepted file types, the
/// official page to get them (if the port names one) and where in the port's own repository this was declared.
/// </summary>
public sealed record PortMods(string Folder, string Location, IReadOnlyList<string> FileTypes, string? SourceUrl, string DeclaredIn, string? NotesPtBr);

[JsonConverter(typeof(JsonStringEnumConverter<PortStatus>))]
public enum PortStatus
{
    JOGAVEL,
    EXPERIMENTAL,
    EM_DESENVOLVIMENTO,
    DESCONTINUADO,
    NAO_AVALIADO,
}

public sealed record RepoRef(string Owner, string Name)
{
    public string Url => $"https://github.com/{Owner}/{Name}";
}

public sealed record WindowsAsset(string Pattern, string? ExcludePattern, bool PreferPrerelease, string PackageType);

public sealed record PortSetup(string? Kind, string? TargetFileName, string? TargetFolder, string? NotesPtBr, string? ExtractCommand, string? ExpectedOutput, IReadOnlyList<string>? RequiredEntries);

public sealed record RequiredGameFile(string Label, string Kind, string? Format, string? CanonicalFormat, IReadOnlyList<AcceptedVersion> AcceptedVersions, string? OfficialDocsUrl);

public sealed record AcceptedVersion(string Region, string Version, string? FileName, string? Sha1, string? Md5, string? Sha256, string? Crc32, string Source);

public sealed record PersistentPath(string Path, string Kind, string Location);

public sealed record ExperiencePresets(string? ConfigFile, string? ConfigFormat, Preset Original, Preset Remasterizado)
{
    /// <summary>The style picker only appears when both styles were confirmed in the port's real configuration.</summary>
    public bool IsVerified => Original.Verified && Remasterizado.Verified && ConfigFile is not null;
}

public sealed record Preset(IReadOnlyDictionary<string, JsonElement> Values, bool Verified);

public sealed record PortLinks(string Repo, string? Docs, string? Community);

public sealed record PortArt(string? Icon, string? Banner, string? ArtLicense);

/// <summary>What the scanner measured on GitHub, and when (stars and dates are a snapshot of that day).</summary>
public sealed record PortEvidence(string? ScannedAt, string? ReleaseTag, string? ReleaseDate, int? Stars, bool Prerelease, string? AssetName, long? AssetSize, string? AssetSha256);

public static class CatalogJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
