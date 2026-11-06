using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

/// <summary>
/// Vertical 600x900 covers from SteamGridDB (the same source RecompLauncher offers for custom art).
/// The API key belongs to the curator: read from SGDB_API_KEY or %USERPROFILE%\.lotusports\steamgriddb.key,
/// never printed, never written to the catalog or the app. Only the image address is stored.
/// </summary>
internal static class SteamGridDb
{
    public static string? ReadKey()
    {
        if (Environment.GetEnvironmentVariable("SGDB_API_KEY") is { Length: > 0 } env) return env.Trim();
        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".lotusports", "steamgriddb.key");
        return File.Exists(file) ? File.ReadAllText(file).Trim() is { Length: > 0 } k ? k : null : null;
    }

    public static HttpClient Client(string key)
    {
        var http = new HttpClient { BaseAddress = new Uri("https://www.steamgriddb.com/api/v2/") };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("LotusPorts-CatalogScout/0.1");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        return http;
    }

    /// <summary>The SteamGridDB game whose name gives the same key as the catalog title, or null (never a guess).</summary>
    public static async Task<(int Id, string Name)?> FindAsync(HttpClient http, string title, Func<string, string> key)
    {
        var data = await GetDataAsync(http, "search/autocomplete/" + Uri.EscapeDataString(title));
        var wanted = key(title);
        foreach (var item in data ?? [])
        {
            var name = item!["name"]!.GetValue<string>();
            if (key(name) == wanted) return (item["id"]!.GetValue<int>(), name);
        }
        return null;
    }

    /// <summary>Best static 600x900 grid (score first, English or no text preferred), or null.</summary>
    public static async Task<string?> BestGridAsync(HttpClient http, int gameId)
    {
        var data = await GetDataAsync(http, $"grids/game/{gameId}?dimensions=600x900&mimes=image/png,image/jpeg&types=static&nsfw=false&humor=false&epilepsy=false");
        return data?
            .Select(g => (Url: g!["url"]?.GetValue<string>(), Score: g["score"]?.GetValue<int>() ?? 0, Lang: g["language"]?.GetValue<string>() ?? "en"))
            .Where(g => g.Url is not null && g.Url.StartsWith("https://cdn2.steamgriddb.com/", StringComparison.Ordinal)
                && (g.Url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || g.Url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || g.Url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(g => g.Lang == "en" ? 0 : 1).ThenByDescending(g => g.Score)
            .Select(g => g.Url)
            .FirstOrDefault();
    }

    private static async Task<JsonArray?> GetDataAsync(HttpClient http, string path)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var response = await http.GetAsync(path);
            if (response.StatusCode == HttpStatusCode.TooManyRequests) { await Task.Delay(5000 * (attempt + 1)); continue; }
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new InvalidOperationException("SteamGridDB recusou a chave (401). Confira a chave salva.");
            if (!response.IsSuccessStatusCode) return null;
            var node = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            return node?["success"]?.GetValue<bool>() == true ? node["data"] as JsonArray : null;
        }
        return null;
    }
}
