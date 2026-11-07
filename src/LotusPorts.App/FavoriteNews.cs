using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Core.Preferences;
using LotusPorts.Infrastructure.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace LotusPorts.App;

public sealed record FavoriteNewsItem(LibraryGame Game, string Tag);

/// <summary>
/// "Novidades nos seus favoritos": favorite games that are not installed get a notice when their recommended port
/// publishes a new version. The first check only remembers each version (no notice for what already existed); checked
/// 40 s after opening and then once a day, only while the app is open. Opening the game page marks it as seen.
/// </summary>
public sealed class FavoriteNews(PlayService play, CatalogService catalog, ISettingsStore settings, ILogger<FavoriteNews> logger)
{
    private const string SeenKey = "favorites.seenReleases";
    private readonly List<FavoriteNewsItem> items = [];
    private DispatcherQueueTimer? timer;
    private DispatcherQueue? ui;

    public event Action? Changed;

    public IReadOnlyList<FavoriteNewsItem> Items => items;

    public void Start(DispatcherQueue dispatcher)
    {
        ui = dispatcher;
        timer = dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(40);
        timer.Tick += async (t, _) =>
        {
            t.Interval = TimeSpan.FromHours(24);
            await CheckAsync();
        };
        timer.Start();
    }

    public async Task CheckAsync()
    {
        var seen = await LoadAsync();
        var news = new List<FavoriteNewsItem>();
        foreach (var game in catalog.Library.Visible(showInReview: false).Where(g => catalog.StateOf(g.Id) is { Favorite: true, Installed: false }).ToList())
        {
            var port = game.Recommended.Port;
            ReleaseLookup lookup;
            try { lookup = await play.LookupAsync(port); }
            catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException)
            {
                logger.LogInformation(ex, "Favorite check failed for {Port}", port.Id);
                continue;
            }
            if (lookup.RateLimitedUntil is not null) break;
            if (lookup.Choice?.Release.Tag is not { } tag) continue;
            if (!seen.TryGetValue(port.Id, out var known)) seen[port.Id] = tag; // first look: just remember it
            else if (known != tag) news.Add(new FavoriteNewsItem(game, tag));
        }
        await SaveAsync(seen);
        items.Clear();
        items.AddRange(news);
        Raise();
    }

    /// <summary>The player opened the game: its new version is no longer news.</summary>
    public async Task MarkSeenAsync(string gameId)
    {
        if (items.FirstOrDefault(i => i.Game.Id == gameId) is not { } item) return;
        var seen = await LoadAsync();
        seen[item.Game.Recommended.Port.Id] = item.Tag;
        await SaveAsync(seen);
        items.Remove(item);
        Raise();
    }

    private async Task<Dictionary<string, string>> LoadAsync()
    {
        try
        {
            return await settings.GetAsync(SeenKey) is { } json ? JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [] : [];
        }
        catch (JsonException) { return []; }
    }

    private Task SaveAsync(Dictionary<string, string> seen) => settings.SetAsync(SeenKey, JsonSerializer.Serialize(seen));

    private void Raise()
    {
        if (ui is null || ui.HasThreadAccess) Changed?.Invoke();
        else ui.TryEnqueue(() => Changed?.Invoke());
    }
}
