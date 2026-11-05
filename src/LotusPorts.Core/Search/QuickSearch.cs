using System.Globalization;

namespace LotusPorts.Core.Search;

public enum SearchEntryKind
{
    Game,
    Action,
}

public sealed record SearchEntry(string Id, SearchEntryKind Kind, string Title, IReadOnlyList<string> Keywords);

/// <summary>Search box filter: ignores case and accents ("configuracoes" finds "Configurações").</summary>
public static class QuickSearch
{
    private const CompareOptions Loose = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public static IReadOnlyList<SearchEntry> Filter(IEnumerable<SearchEntry> entries, string? query, int limit = 8)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var q = query?.Trim();
        if (string.IsNullOrEmpty(q)) return [];
        var compare = CultureInfo.InvariantCulture.CompareInfo;
        bool Contains(string text) => compare.IndexOf(text, q, Loose) >= 0;
        int Rank(SearchEntry e) => compare.IsPrefix(e.Title, q, Loose) ? 0 : Contains(e.Title) ? 1 : e.Keywords.Any(Contains) ? 2 : -1;
        return entries
            .Select(e => (Entry: e, Rank: Rank(e)))
            .Where(x => x.Rank >= 0)
            .OrderBy(x => x.Rank).ThenBy(x => x.Entry.Kind).ThenBy(x => x.Entry.Title, StringComparer.CurrentCultureIgnoreCase)
            .Take(limit)
            .Select(x => x.Entry)
            .ToList();
    }
}
