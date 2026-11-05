using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Lotus.Design.Controls;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Core.Search;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.Database;

namespace LotusPorts.App.ViewModels;

public sealed record SourceRow(CatalogSourceRecord Record, string StateText, BadgeTone Tone, string Detail);

/// <summary>Catálogo: every game to discover, plus the catalog sources.</summary>
public sealed partial class CatalogViewModel(CatalogService catalog, PreferencesService preferences) : ObservableObject
{
    private IReadOnlyList<LibraryGame> visible = [];

    [ObservableProperty] private string summary = "";
    [ObservableProperty] private string officialText = "";
    [ObservableProperty] private bool advancedMode;
    [ObservableProperty] private bool hasUserPorts;
    [ObservableProperty] private bool noMatches;

    public ObservableCollection<GameCardModel> Games { get; } = [];
    public ObservableCollection<SourceRow> Sources { get; } = [];
    public List<PlatformChip> Platforms { get; } = [];
    public CatalogService Service => catalog;

    private string query = "";
    private Platform? platform;

    public async Task LoadAsync()
    {
        var prefs = await preferences.LoadAsync();
        AdvancedMode = prefs.AdvancedMode;
        visible = catalog.Library.Visible(prefs.ShowInReview).ToList();
        Platforms.Clear();
        Platforms.Add(new PlatformChip(null, Strings.Get("FilterAllPlatforms")));
        Platforms.AddRange(visible.Select(g => g.Game.OriginalPlatform).Distinct().Order().Select(p => new PlatformChip(p, CatalogLabels.Platform(p))));
        Refresh();
    }

    public void Filter(string text, Platform? chosen)
    {
        query = text;
        platform = chosen;
        RefreshGames();
    }

    public void Refresh()
    {
        RefreshGames();
        var o = catalog.Official;
        OfficialText = o.Valid ? Strings.Format("CatalogOfficialInfo", o.Version, o.Games, o.Ports) : Strings.Get("CatalogOfficialInvalid");
        var waiting = catalog.Library.Games.Count(g => g.Visibility == GameVisibility.InReview);
        Summary = Strings.Format("CatalogSummary", visible.Count, visible.Sum(g => g.AllPorts.Count())) + (waiting > 0 ? " " + Strings.Format("CatalogWaitingReview", waiting) : "");
        Sources.Clear();
        foreach (var s in catalog.Sources) Sources.Add(Row(s));
        HasUserPorts = catalog.UserCatalog is { Ports.Count: > 0 };
    }

    private void RefreshGames()
    {
        var games = visible.Where(g => platform is null || g.Game.OriginalPlatform == platform);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var entries = games.Select(g => new SearchEntry(g.Id, SearchEntryKind.Game, g.Game.Title, [.. g.AllPorts.Select(p => p.Port.Title), .. g.Game.Genre])).ToList();
            var ids = QuickSearch.Filter(entries, query, int.MaxValue).Select(e => e.Id).ToHashSet();
            games = games.Where(g => ids.Contains(g.Id));
        }
        Games.Clear();
        foreach (var g in games) Games.Add(GameCardModel.From(g, catalog.StateOf(g.Id)));
        NoMatches = Games.Count == 0;
    }

    private static SourceRow Row(CatalogSourceRecord s)
    {
        var (text, tone) = s.ValidationState switch
        {
            SourceValidationState.Ok => (Strings.Get("SourceStateOk"), BadgeTone.Ok),
            SourceValidationState.Warning => (Strings.Get("SourceStateWarning"), BadgeTone.Warning),
            _ => (Strings.Get("SourceStateNotVerified"), BadgeTone.Unknown),
        };
        var parts = new List<string> { s.Url };
        parts.Add(s.Active ? Strings.Get("SourceActive") : Strings.Get("SourceInactive"));
        parts.Add(s.Trusted ? Strings.Get("SourceTrusted") : Strings.Get("SourceUntrusted"));
        if (s.LastUpdatedAt is { } at) parts.Add(Strings.Format("SourceUpdatedAt", at.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)));
        if (s.ValidationMessage is { } message) parts.Add(message);
        return new SourceRow(s, text, tone, string.Join(" · ", parts));
    }

    public static string AddResultMessage(AddSourceResult result) => result.Outcome switch
    {
        AddSourceOutcome.Added => Strings.Format("SourceAdded", result.Source!.Name),
        AddSourceOutcome.AlreadyAdded => Strings.Get("SourceAlreadyAdded"),
        AddSourceOutcome.InvalidLink => Strings.Get("SourceInvalidLink"),
        AddSourceOutcome.NotFound => Strings.Get("SourceNotFound"),
        AddSourceOutcome.GameFilesRejected => Strings.Get("SourceGameFilesRejected"),
        AddSourceOutcome.RateLimited => result.RetryAt is { } at
            ? Strings.Format("SourceRateLimitedUntil", at.ToLocalTime().ToString("t", CultureInfo.CurrentCulture))
            : Strings.Get("SourceRateLimited"),
        AddSourceOutcome.NetworkError => Strings.Get("SourceNetworkError"),
        _ => Strings.Format("SourceInvalid", result.Issues.Count),
    };
}
