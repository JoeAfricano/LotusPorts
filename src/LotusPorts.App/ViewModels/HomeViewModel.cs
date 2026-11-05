using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Catalog;

namespace LotusPorts.App.ViewModels;

public sealed record ConsoleTile(Platform Platform, string Label, string CountText);

public sealed record GenreChip(string Genre, string Label);

/// <summary>
/// Início: the storefront. Every order comes from facts measured on GitHub (stars, release dates)
/// or from the player's own history, never from invented popularity.
/// </summary>
public sealed partial class HomeViewModel(CatalogService catalog, PreferencesService preferences) : ObservableObject
{
    public const int ShelfSize = 16;

    [ObservableProperty] private string featuredId = "";
    [ObservableProperty] private string featuredTitle = "";
    [ObservableProperty] private string featuredDetail = "";
    [ObservableProperty] private bool hasFeatured;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private bool hasConsoles;
    [ObservableProperty] private bool hasGenres;

    public ObservableCollection<ShelfModel> Shelves { get; } = [];
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

        Shelves.Clear();
        Add("HomePopular", playable.Where(g => Stars(g) > 0).OrderByDescending(Stars), g => Strings.Format("CardStars", Compact(Stars(g))));
        Add("HomeUpdated", playable.Where(g => Latest(g) is not null).OrderByDescending(g => Latest(g)), g => Strings.Format("CardUpdated", DateText(Latest(g)!)));
        if (catalog.NewSince is { } since)
            Add("HomeNew", playable.Where(g => catalog.StateOf(g.Id) is { DismissedFromNew: false, FirstSeen: { } seen } && seen > since).OrderByDescending(g => catalog.StateOf(g.Id).FirstSeen), null);
        Add("HomeSoon", visible.Where(ShelfBuilder.IsStillBeingBuilt), null);

        Consoles.Clear();
        foreach (var group in visible.GroupBy(g => g.Game.OriginalPlatform).OrderByDescending(g => g.Count()).ThenBy(g => CatalogLabels.Platform(g.Key)))
            Consoles.Add(new ConsoleTile(group.Key, CatalogLabels.Platform(group.Key), Strings.Format(group.Count() == 1 ? "HomeConsoleOne" : "HomeConsoleMany", group.Count())));

        Genres.Clear();
        foreach (var genre in visible.SelectMany(g => g.Game.Genre).GroupBy(x => x, StringComparer.OrdinalIgnoreCase).OrderByDescending(g => g.Count()))
            Genres.Add(new GenreChip(genre.Key, $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(genre.Key)} ({genre.Count()})"));
        HasConsoles = Consoles.Count > 0;
        HasGenres = Genres.Count > 0;
    }

    private void Add(string titleKey, IEnumerable<LibraryGame> games, Func<LibraryGame, string>? line)
    {
        var cards = games.Take(ShelfSize).Select(g =>
        {
            var card = GameCardModel.From(g, catalog.StateOf(g.Id));
            return line is null ? card : card with { StateLine = line(g) };
        }).ToList();
        if (cards.Count > 0) Shelves.Add(new ShelfModel(Strings.Get(titleKey), cards));
    }

    private static int Stars(LibraryGame g) => g.AllPorts.Max(p => p.Port.Evidence?.Stars ?? 0);

    private static string? Latest(LibraryGame g) => g.AllPorts.Select(p => p.Port.Evidence?.ReleaseDate).Where(d => d is not null).Max();

    private static string Compact(int n) => n >= 1000
        ? Strings.Format("CompactThousands", (n / 1000.0).ToString(n >= 10_000 ? "0" : "0.#", CultureInfo.CurrentCulture))
        : n.ToString(CultureInfo.CurrentCulture);

    private static string DateText(string iso) =>
        DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.ToString("d MMM yyyy", CultureInfo.CurrentCulture) : iso;
}
