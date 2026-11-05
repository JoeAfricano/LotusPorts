namespace LotusPorts.Catalog;

public enum CatalogOrigin
{
    Official,
    External,
    User,
}

/// <summary>A validated catalog plus where it came from.</summary>
public sealed record CatalogSourceBundle(string SourceId, string SourceName, CatalogOrigin Origin, bool Trusted, CatalogBundle Bundle);

/// <summary>One way to play a game, with the origin badge the interface must show.</summary>
public sealed record PortChoice(Port Port, CatalogOrigin Origin, string SourceName, bool Trusted, string? DifferencePtBr);

public enum GameVisibility
{
    /// <summary>Official and approved by José.</summary>
    Public,

    /// <summary>Official but still waiting for review: only with "Mostrar jogos em revisão" (advanced mode).</summary>
    InReview,

    /// <summary>From an imported source or added by the user: shown with "Fonte externa" / "Adicionado por você".</summary>
    External,

    Hidden,
}

public sealed record LibraryGame(Game Game, CatalogOrigin Origin, string SourceName, bool Trusted, GameVisibility Visibility, PortChoice Recommended, IReadOnlyList<PortChoice> Alternatives)
{
    public string Id => Game.Id;

    public IEnumerable<PortChoice> AllPorts => Alternatives.Prepend(Recommended);
}

public sealed record LibraryMergeWarning(string SourceName, string Message);

/// <summary>
/// Merges the official catalog with imported sources and the user's own ports.
/// The official catalog wins every id conflict; a conflicting external port becomes an extra "other way to play".
/// </summary>
public sealed class CatalogLibrary
{
    private CatalogLibrary(IReadOnlyList<LibraryGame> games, IReadOnlyList<LibraryMergeWarning> warnings)
    {
        Games = games;
        Warnings = warnings;
    }

    public IReadOnlyList<LibraryGame> Games { get; }
    public IReadOnlyList<LibraryMergeWarning> Warnings { get; }

    public static CatalogLibrary Empty { get; } = new([], []);

    public LibraryGame? Find(string gameId) => Games.FirstOrDefault(g => g.Id == gameId);

    public IEnumerable<LibraryGame> Visible(bool showInReview) =>
        Games.Where(g => g.Visibility is GameVisibility.Public or GameVisibility.External || (showInReview && g.Visibility == GameVisibility.InReview));

    public static CatalogLibrary Build(IEnumerable<CatalogSourceBundle> sources)
    {
        var ordered = sources.OrderBy(s => s.Origin).ToList();
        var games = new Dictionary<string, LibraryGame>();
        var portOwners = new Dictionary<string, string>();
        var warnings = new List<LibraryMergeWarning>();

        foreach (var source in ordered)
        {
            var ports = source.Bundle.Ports.ToDictionary(p => p.Id);
            foreach (var game in source.Bundle.Games)
            {
                var choices = new[] { (Id: game.RecommendedPortId, Difference: (string?)null) }
                    .Concat(game.Alternatives.Select(a => (Id: a.PortId, Difference: a.DifferencePtBr)))
                    .Where(c => ports.ContainsKey(c.Id))
                    .Select(c => new PortChoice(ports[c.Id], source.Origin, source.SourceName, source.Trusted || source.Origin == CatalogOrigin.Official, c.Difference))
                    .ToList();

                // Port ids are global: a second source cannot redefine one.
                var accepted = new List<PortChoice>();
                foreach (var choice in choices)
                {
                    if (portOwners.TryGetValue(choice.Port.Id, out var owner))
                    {
                        warnings.Add(new(source.SourceName, $"Port {choice.Port.Id} already defined by {owner}; ignored."));
                        continue;
                    }
                    portOwners[choice.Port.Id] = source.SourceName;
                    accepted.Add(choice);
                }
                if (accepted.Count == 0) continue;

                if (games.TryGetValue(game.Id, out var existing))
                {
                    // Official (or earlier) game wins: the newcomer only adds other ways to play.
                    games[game.Id] = existing with { Alternatives = [.. existing.Alternatives, .. accepted] };
                    warnings.Add(new(source.SourceName, $"Game {game.Id} already exists; its ports were added as other ways to play."));
                    continue;
                }

                games[game.Id] = new LibraryGame(game, source.Origin, source.SourceName, source.Trusted || source.Origin == CatalogOrigin.Official,
                    VisibilityOf(game, source.Origin), accepted[0], accepted.Skip(1).ToList());
            }
        }

        return new CatalogLibrary(games.Values.OrderBy(g => g.Game.Title, StringComparer.CurrentCultureIgnoreCase).ToList(), warnings);
    }

    private static GameVisibility VisibilityOf(Game game, CatalogOrigin origin) => origin switch
    {
        CatalogOrigin.Official => game.Review.State switch
        {
            ReviewState.Approved => GameVisibility.Public,
            ReviewState.Pending => GameVisibility.InReview,
            _ => GameVisibility.Hidden,
        },
        _ => game.Review.State == ReviewState.Rejected ? GameVisibility.Hidden : GameVisibility.External,
    };
}
