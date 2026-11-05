using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Catalog;

namespace LotusPorts.App.ViewModels;

public enum ViewState
{
    Loading,
    Empty,
    Ready,
}

public sealed record PlatformChip(Platform? Platform, string Label);

/// <summary>
/// Biblioteca: the player's own games (favorites, installed, ready, played). Discovering new games
/// happens in Início and Catálogo.
/// </summary>
public sealed partial class LibraryViewModel(CatalogService catalog, PreferencesService preferences) : ObservableObject
{
    private IReadOnlyList<LibraryGame> mine = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsEmpty), nameof(IsReady))]
    private ViewState state;

    [ObservableProperty] private string emptyTitle = "";
    [ObservableProperty] private string emptyMessage = "";
    [ObservableProperty] private string allLabel = "";
    [ObservableProperty] private string favoritesLabel = "";
    [ObservableProperty] private string installedLabel = "";

    public bool IsLoading => State == ViewState.Loading;
    public bool IsEmpty => State == ViewState.Empty;
    public bool IsReady => State == ViewState.Ready;

    public LibraryTab Tab { get; private set; } = LibraryTab.All;

    public ObservableCollection<ShelfModel> Shelves { get; } = [];
    public SearchViewModel Search { get; } = new();

    public async Task LoadAsync()
    {
        State = ViewState.Loading;
        var showInReview = (await preferences.LoadAsync()).ShowInReview;
        var visible = catalog.Library.Visible(showInReview).ToList();
        Search.Load(visible);
        mine = visible.Where(g => IsMine(catalog.StateOf(g.Id))).ToList();
        Rebuild();
    }

    public void SetTab(LibraryTab tab)
    {
        Tab = tab;
        Rebuild();
    }

    private static bool IsMine(GameUserState s) => s.Favorite || s.Installed || s.FileRecognized || s.LastPlayed is not null;

    private void Rebuild()
    {
        AllLabel = Strings.Format("TabAll", mine.Count);
        FavoritesLabel = Strings.Format("TabFavorites", mine.Count(g => catalog.StateOf(g.Id).Favorite));
        InstalledLabel = Strings.Format("TabInstalled", mine.Count(g => catalog.StateOf(g.Id).Installed));

        var filtered = ShelfBuilder.Apply(mine, new LibraryFilter(Tab), catalog.StateOf);
        Shelves.Clear();
        var resume = filtered.Where(g => catalog.StateOf(g.Id).LastPlayed is not null).OrderByDescending(g => catalog.StateOf(g.Id).LastPlayed).ToList();
        var ready = filtered.Where(g => catalog.StateOf(g.Id) is { Installed: true } or { FileRecognized: true }).Except(resume).ToList();
        var favorites = filtered.Where(g => catalog.StateOf(g.Id).Favorite).Except(resume).Except(ready).ToList();
        Add("ShelfContinue", resume);
        Add("ShelfReady", ready);
        Add("ShelfFavorites", favorites);

        if (Shelves.Count > 0) { State = ViewState.Ready; return; }
        (EmptyTitle, EmptyMessage) = mine.Count == 0
            ? (Strings.Get("LibraryYoursEmptyTitle"), Strings.Get("LibraryYoursEmptyMessage"))
            : (Strings.Get("LibraryNoMatchTitle"), Strings.Get("LibraryNoMatchMessage"));
        State = ViewState.Empty;
    }

    private void Add(string titleKey, IReadOnlyList<LibraryGame> games)
    {
        if (games.Count > 0) Shelves.Add(new ShelfModel(Strings.Get(titleKey), games.Select(g => GameCardModel.From(g, catalog.StateOf(g.Id))).ToList()));
    }
}
