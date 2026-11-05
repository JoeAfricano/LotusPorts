using Lotus.Design.Controls;
using LotusPorts.Catalog;

namespace LotusPorts.App;

/// <summary>Plain-language texts for catalog values (no jargon in the default interface).</summary>
public static class CatalogLabels
{
    public static string Status(PortStatus status) => Strings.Get("Status" + status);

    public static BadgeTone StatusTone(PortStatus status) => status switch
    {
        PortStatus.JOGAVEL => BadgeTone.Ok,
        PortStatus.EXPERIMENTAL => BadgeTone.Warning,
        PortStatus.EM_DESENVOLVIMENTO => BadgeTone.Accent,
        PortStatus.DESCONTINUADO => BadgeTone.Failure,
        _ => BadgeTone.Unknown,
    };

    public static string Platform(Platform platform) => Strings.Get("Platform" + platform);

    public static string PortType(string type) => Strings.Get("PortType" + type.Replace("-", "", StringComparison.Ordinal));

    public static string Setup(string? kind) => Strings.Get("Setup" + (kind?.Replace("-", "", StringComparison.Ordinal) ?? "Unknown"));

    /// <summary>Badge for games that are not official-and-approved. Null when nothing needs saying.</summary>
    public static string? Origin(LibraryGame game) => game.Visibility switch
    {
        GameVisibility.InReview => Strings.Get("OriginInReview"),
        GameVisibility.External when game.Origin == CatalogOrigin.User => Strings.Get("OriginUser"),
        GameVisibility.External => Strings.Get("OriginExternal"),
        _ => null,
    };

    public static string Origin(PortChoice choice) => choice.Origin switch
    {
        CatalogOrigin.Official => Strings.Get("OriginOfficial"),
        CatalogOrigin.User => Strings.Get("OriginUser"),
        _ => choice.Trusted ? Strings.Format("OriginExternalTrusted", choice.SourceName) : Strings.Format("OriginExternalUntrusted", choice.SourceName),
    };
}

/// <summary>One cover in a shelf or grid.</summary>
public sealed record GameCardModel(
    string Id,
    string Title,
    string PlatformKey,
    string PlatformLabel,
    string StatusText,
    BadgeTone StatusTone,
    string? OriginText,
    string StateLine,
    bool Favorite)
{
    public bool HasOrigin => OriginText is not null;

    public static GameCardModel From(LibraryGame game, GameUserState state) => new(
        game.Id,
        game.Game.Title,
        game.Game.OriginalPlatform.ToString(),
        CatalogLabels.Platform(game.Game.OriginalPlatform),
        CatalogLabels.Status(game.Recommended.Port.Status),
        CatalogLabels.StatusTone(game.Recommended.Port.Status),
        CatalogLabels.Origin(game),
        // Installing and game files arrive in phases 2 and 3: nothing is claimed before it is checked.
        state.Installed ? Strings.Get("StateInstalled") : Strings.Get("StateNotInstalled"),
        state.Favorite);
}

public sealed record ShelfModel(string Title, IReadOnlyList<GameCardModel> Games);
