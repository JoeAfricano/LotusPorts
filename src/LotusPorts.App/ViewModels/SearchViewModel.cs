using System.Collections.ObjectModel;
using LotusPorts.Catalog;
using LotusPorts.Core.Search;

namespace LotusPorts.App.ViewModels;

/// <summary>The search box (Ctrl+K): games by name, port, platform or genre, plus screens.</summary>
public sealed class SearchViewModel
{
    private IReadOnlyList<SearchEntry> searchable = [];

    public ObservableCollection<SearchEntry> Suggestions { get; } = [];

    public void Load(IEnumerable<LibraryGame> games)
    {
        searchable =
        [
            .. games.Select(g => new SearchEntry(g.Id, SearchEntryKind.Game, g.Game.Title,
                [CatalogLabels.Platform(g.Game.OriginalPlatform), .. g.AllPorts.Select(p => p.Port.Title), .. g.Game.Genre])),
            Action(Pages.Home, "NavHome", "SearchKeywordsHome"),
            Action(Pages.Library, "NavLibrary", "SearchKeywordsLibrary"),
            Action(Pages.Catalog, "NavCatalog", "SearchKeywordsCatalog"),
            Action(Pages.Downloads, "NavDownloads", "SearchKeywordsDownloads"),
            Action(Pages.Lotus, "NavLotus", "SearchKeywordsLotus"),
            Action(Pages.Settings, "NavSettings", "SearchKeywordsSettings"),
            Action(Pages.Help, "NavHelp", "SearchKeywordsHelp"),
        ];
    }

    public void Update(string text)
    {
        Suggestions.Clear();
        if (string.IsNullOrWhiteSpace(text)) return;
        var found = QuickSearch.Filter(searchable, text);
        if (found.Count == 0) Suggestions.Add(new SearchEntry("", SearchEntryKind.Action, Strings.Get("SearchNoResults"), []));
        foreach (var entry in found) Suggestions.Add(entry);
    }

    /// <summary>Opens what was chosen. False when there was nothing to open.</summary>
    public bool Open(SearchEntry? chosen)
    {
        var entry = chosen ?? Suggestions.FirstOrDefault(s => s.Id.Length > 0);
        if (entry is not { Id.Length: > 0 }) return false;
        if (entry.Kind == SearchEntryKind.Game) App.MainWindow?.Navigate(Pages.Game, entry.Id);
        else App.MainWindow?.Navigate(entry.Id);
        return true;
    }

    private static SearchEntry Action(string page, string titleKey, string keywordsKey) =>
        new(page, SearchEntryKind.Action, Strings.Get(titleKey), Strings.Get(keywordsKey).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
}
