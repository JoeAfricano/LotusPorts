using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Search;
using LotusPorts.Infrastructure.Catalog;

namespace LotusPorts.App.ViewModels;

public enum ViewState
{
    Loading,
    Empty,
    Ready,
}

public sealed record PlatformChip(Platform? Platform, string Label);

/// <summary>Biblioteca: tabs, chips and shelves over the merged catalog. Nothing unreviewed shows unless asked for.</summary>
public sealed partial class LibraryViewModel(CatalogService catalog, PreferencesService preferences) : ObservableObject
{
    private IReadOnlyList<SearchEntry> searchable = [];
    private IReadOnlyList<LibraryGame> visible = [];
    private bool showInReview;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsEmpty), nameof(IsReady))]
    private ViewState state;

    [ObservableProperty] private string emptyTitle = "";
    [ObservableProperty] private string emptyMessage = "";
    [ObservableProperty] private string allLabel = "";
    [ObservableProperty] private string favoritesLabel = "";
    [ObservableProperty] private string installedLabel = "";
    [ObservableProperty] private string notInstalledLabel = "";
    [ObservableProperty] private bool hasNewShelf;

    public bool IsLoading => State == ViewState.Loading;
    public bool IsEmpty => State == ViewState.Empty;
    public bool IsReady => State == ViewState.Ready;

    public LibraryTab Tab { get; private set; } = LibraryTab.All;
    public Platform? PlatformFilter { get; private set; }
    public string? GenreFilter { get; private set; }

    public ObservableCollection<ShelfModel> Shelves { get; } = [];
    public ObservableCollection<SearchEntry> Suggestions { get; } = [];
    public List<PlatformChip> Platforms { get; } = [];
    public List<string> Genres { get; } = [];

    public async Task LoadAsync()
    {
        State = ViewState.Loading;
        showInReview = (await preferences.LoadAsync()).ShowInReview;
        visible = catalog.Library.Visible(showInReview).ToList();

        Platforms.Clear();
        Platforms.Add(new PlatformChip(null, Strings.Get("FilterAllPlatforms")));
        Platforms.AddRange(visible.Select(g => g.Game.OriginalPlatform).Distinct().Order().Select(p => new PlatformChip(p, CatalogLabels.Platform(p))));
        Genres.Clear();
        Genres.Add(Strings.Get("FilterAllGenres"));
        Genres.AddRange(visible.SelectMany(g => g.Game.Genre).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.CurrentCultureIgnoreCase));

        searchable =
        [
            .. visible.Select(g => new SearchEntry(g.Id, SearchEntryKind.Game, g.Game.Title,
                [CatalogLabels.Platform(g.Game.OriginalPlatform), .. g.AllPorts.Select(p => p.Port.Title), .. g.Game.Genre])),
            Action(Pages.Library, "NavLibrary", "SearchKeywordsLibrary"),
            Action(Pages.Catalog, "NavCatalog", "SearchKeywordsCatalog"),
            Action(Pages.Downloads, "NavDownloads", "SearchKeywordsDownloads"),
            Action(Pages.Lotus, "NavLotus", "SearchKeywordsLotus"),
            Action(Pages.Settings, "NavSettings", "SearchKeywordsSettings"),
            Action(Pages.Help, "NavHelp", "SearchKeywordsHelp"),
        ];
        Rebuild();
    }

    public void SetTab(LibraryTab tab) { Tab = tab; Rebuild(); }
    public void SetPlatform(Platform? platform) { PlatformFilter = platform; Rebuild(); }
    public void SetGenre(string? genre) { GenreFilter = genre; Rebuild(); }

    public async Task SetFavoriteAsync(string gameId, bool favorite)
    {
        await catalog.SetFavoriteAsync(gameId, favorite);
        Rebuild();
    }

    public async Task DismissNewAsync()
    {
        foreach (var game in Shelves.FirstOrDefault(s => s.Title == Strings.Get("ShelfNew"))?.Games ?? [])
            await catalog.DismissFromNewAsync(game.Id);
        Rebuild();
    }

    private void Rebuild()
    {
        var byChips = ShelfBuilder.Apply(visible, new LibraryFilter(LibraryTab.All, PlatformFilter, GenreFilter), catalog.StateOf);
        AllLabel = Strings.Format("TabAll", byChips.Count);
        FavoritesLabel = Strings.Format("TabFavorites", byChips.Count(g => catalog.StateOf(g.Id).Favorite));
        InstalledLabel = Strings.Format("TabInstalled", byChips.Count(g => catalog.StateOf(g.Id).Installed));
        NotInstalledLabel = Strings.Format("TabNotInstalled", byChips.Count(g => !catalog.StateOf(g.Id).Installed));

        var filtered = ShelfBuilder.Apply(byChips, new LibraryFilter(Tab), catalog.StateOf);
        var shelves = ShelfBuilder.Build(filtered, catalog.StateOf, catalog.NewSince);
        Shelves.Clear();
        foreach (var shelf in shelves)
            Shelves.Add(new ShelfModel(Strings.Get("Shelf" + shelf.Kind), shelf.Games.Select(g => GameCardModel.From(g, catalog.StateOf(g.Id))).ToList()));
        HasNewShelf = shelves.Any(s => s.Kind == ShelfKind.New);

        if (Shelves.Count > 0)
        {
            State = ViewState.Ready;
            return;
        }
        var waiting = catalog.Library.Games.Count(g => g.Visibility == GameVisibility.InReview);
        (EmptyTitle, EmptyMessage) = visible.Count == 0 && waiting > 0 && !showInReview
            ? (Strings.Get("LibraryNothingReviewedTitle"), Strings.Format("LibraryNothingReviewedMessage", waiting))
            : visible.Count == 0
                ? (Strings.Get("LibraryEmptyTitle"), Strings.Get("LibraryEmptyMessage"))
                : (Strings.Get("LibraryNoMatchTitle"), Strings.Get("LibraryNoMatchMessage"));
        State = ViewState.Empty;
    }

    public void UpdateSuggestions(string text)
    {
        Suggestions.Clear();
        if (string.IsNullOrWhiteSpace(text)) return;
        var found = QuickSearch.Filter(searchable, text);
        if (found.Count == 0) Suggestions.Add(new SearchEntry("", SearchEntryKind.Action, Strings.Get("SearchNoResults"), []));
        foreach (var entry in found) Suggestions.Add(entry);
    }

    private static SearchEntry Action(string page, string titleKey, string keywordsKey) =>
        new(page, SearchEntryKind.Action, Strings.Get(titleKey), Strings.Get(keywordsKey).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
}
