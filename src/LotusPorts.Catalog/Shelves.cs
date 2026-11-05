namespace LotusPorts.Catalog;

public enum ShelfKind
{
    /// <summary>Chegou! — games that arrived after the player started using the app.</summary>
    New,

    /// <summary>Continuar jogando — by last session.</summary>
    Continue,

    /// <summary>Prontos para jogar — installed or with the game file recognized.</summary>
    Ready,

    /// <summary>Para descobrir — every other playable game.</summary>
    Discover,

    /// <summary>Em breve — ports still being built. The only place these appear.</summary>
    Soon,
}

public enum LibraryTab
{
    All,
    Favorites,
    Installed,
    NotInstalled,
}

public sealed record Shelf(ShelfKind Kind, IReadOnlyList<LibraryGame> Games);

/// <summary>What the player has done with a game. Filled by later phases (install, files, sessions).</summary>
public sealed record GameUserState(bool Favorite, bool Installed, bool FileRecognized, DateTimeOffset? LastPlayed, DateTimeOffset? FirstSeen, bool DismissedFromNew);

public sealed record LibraryFilter(LibraryTab Tab = LibraryTab.All, Platform? Platform = null, string? Genre = null);

public static class ShelfBuilder
{
    public const int NewShelfLimit = 3;

    public static bool IsStillBeingBuilt(LibraryGame game) => game.Recommended.Port.Status == PortStatus.EM_DESENVOLVIMENTO;

    public static IReadOnlyList<LibraryGame> Apply(IEnumerable<LibraryGame> games, LibraryFilter filter, Func<string, GameUserState> state) =>
        games.Where(g =>
        {
            var s = state(g.Id);
            var tabOk = filter.Tab switch
            {
                LibraryTab.Favorites => s.Favorite,
                LibraryTab.Installed => s.Installed,
                LibraryTab.NotInstalled => !s.Installed,
                _ => true,
            };
            return tabOk
                && (filter.Platform is null || g.Game.OriginalPlatform == filter.Platform)
                && (filter.Genre is null || g.Game.Genre.Contains(filter.Genre, StringComparer.OrdinalIgnoreCase));
        }).ToList();

    /// <param name="newSince">Games first seen after this moment are "new". Null on the very first catalog load, so nothing is "new".</param>
    public static IReadOnlyList<Shelf> Build(IEnumerable<LibraryGame> games, Func<string, GameUserState> state, DateTimeOffset? newSince)
    {
        var list = games.ToList();
        var soon = list.Where(IsStillBeingBuilt).ToList();
        var playable = list.Except(soon).ToList();

        var fresh = newSince is null ? [] : playable
            .Where(g => state(g.Id) is { DismissedFromNew: false, FirstSeen: { } seen } && seen > newSince)
            .OrderByDescending(g => state(g.Id).FirstSeen)
            .Take(NewShelfLimit)
            .ToList();
        var resume = playable.Where(g => state(g.Id).LastPlayed is not null).OrderByDescending(g => state(g.Id).LastPlayed).ToList();
        var ready = playable.Where(g => state(g.Id) is { Installed: true } or { FileRecognized: true }).Except(resume).ToList();
        var discover = playable.Except(resume).Except(ready).ToList();

        return new[]
        {
            new Shelf(ShelfKind.New, fresh),
            new Shelf(ShelfKind.Continue, resume),
            new Shelf(ShelfKind.Ready, ready),
            new Shelf(ShelfKind.Discover, discover),
            new Shelf(ShelfKind.Soon, soon),
        }.Where(s => s.Games.Count > 0).ToList();
    }
}
