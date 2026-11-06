using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Catalog;

namespace LotusPorts.App.ViewModels;

public sealed record ConsoleTile(Platform Platform, string Label, string CountText);

public sealed record GenreChip(string Genre, string Label);

/// <summary>Order used by Catálogo and by the "Ver todos" links on Início.</summary>
public enum CatalogSort
{
    Name,
    Popular,
    Updated,
}

/// <summary>
/// Início: the storefront, laid out as in the premium mockups (sections with a fixed number of items and
/// "Ver todos"). Every order comes from facts measured on GitHub or from the player's own history.
/// </summary>
public sealed partial class HomeViewModel(CatalogService catalog, PreferencesService preferences) : ObservableObject
{
    public const int MaxCoverRow = 8;
    public const int MaxTileColumns = 4;

    private List<LibraryGame> popular = [];
    private List<LibraryGame> updated = [];
    private List<LibraryGame> fresh = [];
    private List<LibraryGame> soon = [];
    private int tileColumns = 3;
    private int coverColumns = 6;

    /// <summary>"Mais populares" is one full row: as many covers as the width fits, never a lone card below.</summary>
    public void SetCoverColumns(int columns)
    {
        columns = Math.Clamp(columns, 1, MaxCoverRow);
        if (columns == coverColumns) return;
        coverColumns = columns;
        if (popular.Count > 0) Fill(Popular, popular.Take(columns), PopularLine);
    }

    /// <summary>The compact lists show two full rows for the columns that fit.</summary>
    public void SetTileColumns(int columns)
    {
        columns = Math.Clamp(columns, 1, MaxTileColumns);
        if (columns == tileColumns) return;
        tileColumns = columns;
        FillTiles();
    }

    private void FillTiles()
    {
        var count = tileColumns * 2;
        HasUpdated = Fill(Updated, updated.Take(count), UpdatedLine);
        HasFresh = Fill(Fresh, fresh.Take(count), null);
        HasSoon = Fill(Soon, soon.Take(count), null);
    }

    private static string UpdatedLine(LibraryGame g) => Strings.Format("CardUpdated", DateText(Latest(g)!));

    private static string PopularLine(LibraryGame g) => Strings.Format("CardStars", Compact(Stars(g)));

    [ObservableProperty] private string featuredId = "";
    [ObservableProperty] private string featuredTitle = "";
    [ObservableProperty] private string featuredDetail = "";
    [ObservableProperty] private bool hasFeatured;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private bool hasPopular;
    [ObservableProperty] private bool hasUpdated;
    [ObservableProperty] private bool hasFresh;
    [ObservableProperty] private bool hasSoon;
    [ObservableProperty] private bool hasConsoles;
    [ObservableProperty] private bool hasGenres;

    public ObservableCollection<GameCardModel> Popular { get; } = [];
    public ObservableCollection<GameCardModel> Updated { get; } = [];
    public ObservableCollection<GameCardModel> Fresh { get; } = [];
    public ObservableCollection<GameCardModel> Soon { get; } = [];
    public ObservableCollection<ConsoleTile> Consoles { get; } = [];
    public ObservableCollection<GenreChip> Genres { get; } = [];

    public async Task LoadAsync()
    {
        var showInReview = (await preferences.LoadAsync()).ShowInReview;
        var visible = catalog.Library.Visible(showInReview).ToList();
        IsEmpty = visible.Count == 0;
        var playable = visible.Where(g => !ShelfBuilder.IsStillBeingBuilt(g)).ToList();

        // Featured: the most recent release among the games shown (a measured date, not a pick).
        var featured = playable.Where(g => Latest(g) is not null).OrderByDescending(g => Latest(g)).FirstOrDefault();
        HasFeatured = featured is not null;
        if (featured is not null)
        {
            var port = featured.AllPorts.Where(p => p.Port.Evidence?.ReleaseDate is not null).OrderByDescending(p => p.Port.Evidence!.ReleaseDate).First().Port;
            FeaturedId = featured.Id;
            FeaturedTitle = featured.Game.Title;
            FeaturedDetail = Strings.Format("HomeFeaturedDetail", port.Title, port.Evidence!.ReleaseTag ?? "", DateText(port.Evidence.ReleaseDate!), CatalogLabels.Platform(featured.Game.OriginalPlatform));
        }

        popular = Order(playable, CatalogSort.Popular).Take(MaxCoverRow).ToList();
        HasPopular = Fill(Popular, popular.Take(coverColumns), PopularLine);
        var most = MaxTileColumns * 2;
        updated = Order(playable, CatalogSort.Updated).Take(most).ToList();
        fresh = catalog.NewSince is { } since
            ? playable.Where(g => catalog.StateOf(g.Id) is { DismissedFromNew: false, FirstSeen: { } seen } && seen > since)
                .OrderByDescending(g => catalog.StateOf(g.Id).FirstSeen).Take(most).ToList()
            : [];
        soon = visible.Where(ShelfBuilder.IsStillBeingBuilt).Take(most).ToList();
        FillTiles();

        Consoles.Clear();
        foreach (var group in visible.GroupBy(g => g.Game.OriginalPlatform).OrderByDescending(g => g.Count()).ThenBy(g => CatalogLabels.Platform(g.Key)))
            Consoles.Add(new ConsoleTile(group.Key, CatalogLabels.Platform(group.Key), Strings.Format(group.Count() == 1 ? "HomeConsoleOne" : "HomeConsoleMany", group.Count())));
        Genres.Clear();
        foreach (var genre in visible.SelectMany(g => g.Game.Genre).GroupBy(x => x, StringComparer.OrdinalIgnoreCase).OrderByDescending(g => g.Count()))
            Genres.Add(new GenreChip(genre.Key, $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(genre.Key)} ({genre.Count()})"));
        HasConsoles = Consoles.Count > 0;
        HasGenres = Genres.Count > 0;
    }

    /// <summary>The same orders the Catálogo offers, so "Ver todos" continues the list the player saw.</summary>
    public static IEnumerable<LibraryGame> Order(IEnumerable<LibraryGame> games, CatalogSort sort) => sort switch
    {
        CatalogSort.Popular => games.Where(g => Stars(g) > 0).OrderByDescending(Stars).ThenBy(g => g.Game.Title, StringComparer.CurrentCultureIgnoreCase),
        CatalogSort.Updated => games.Where(g => Latest(g) is not null).OrderByDescending(g => Latest(g)).ThenBy(g => g.Game.Title, StringComparer.CurrentCultureIgnoreCase),
        _ => games.OrderBy(g => g.Game.Title, StringComparer.CurrentCultureIgnoreCase),
    };

    private bool Fill(ObservableCollection<GameCardModel> target, IEnumerable<LibraryGame> games, Func<LibraryGame, string>? line)
    {
        target.Clear();
        foreach (var g in games)
        {
            var card = GameCardModel.From(g, catalog.StateOf(g.Id));
            target.Add(line is null ? card : card with { StateLine = line(g) });
        }
        return target.Count > 0;
    }

    public static int Stars(LibraryGame g) => g.AllPorts.Max(p => p.Port.Evidence?.Stars ?? 0);

    public static string? Latest(LibraryGame g) => g.AllPorts.Select(p => p.Port.Evidence?.ReleaseDate).Where(d => d is not null).Max();

    public static string Compact(int n) => n >= 1000
        ? Strings.Format("CompactThousands", (n / 1000.0).ToString(n >= 10_000 ? "0" : "0.#", CultureInfo.CurrentCulture))
        : n.ToString(CultureInfo.CurrentCulture);

    public static string DateText(string iso) =>
        DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.ToString("d MMM yyyy", CultureInfo.CurrentCulture) : iso;
}
