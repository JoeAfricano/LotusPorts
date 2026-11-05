using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LotusPorts.Core.Search;

namespace LotusPorts.App.ViewModels;

public enum ViewState
{
    Loading,
    Empty,
    Ready,
}

/// <summary>Biblioteca (Estante). Phase 0 has no catalog yet, so it honestly ends empty.</summary>
public sealed partial class LibraryViewModel : ObservableObject
{
    private readonly IReadOnlyList<SearchEntry> searchable;

    public LibraryViewModel()
    {
        searchable =
        [
            Action(Pages.Library, "NavLibrary", "SearchKeywordsLibrary"),
            Action(Pages.Catalog, "NavCatalog", "SearchKeywordsCatalog"),
            Action(Pages.Downloads, "NavDownloads", "SearchKeywordsDownloads"),
            Action(Pages.Lotus, "NavLotus", "SearchKeywordsLotus"),
            Action(Pages.Settings, "NavSettings", "SearchKeywordsSettings"),
            Action(Pages.Help, "NavHelp", "SearchKeywordsHelp"),
        ];
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsEmpty))]
    private ViewState state;

    public bool IsLoading => State == ViewState.Loading;
    public bool IsEmpty => State == ViewState.Empty;

    public ObservableCollection<SearchEntry> Suggestions { get; } = [];

    public void Load() => State = ViewState.Empty;

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
