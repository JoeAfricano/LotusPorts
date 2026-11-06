using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// Matches catalog games to box art in the libretro-thumbnails collection on GitHub and stores only the
/// address. The app fetches and caches the image when it shows it; nothing is bundled.
/// </summary>
internal static partial class Covers
{
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static readonly Dictionary<string, string> Repos = new()
    {
        ["N64"] = "Nintendo_-_Nintendo_64", ["SNES"] = "Nintendo_-_Super_Nintendo_Entertainment_System", ["NES"] = "Nintendo_-_Nintendo_Entertainment_System",
        ["GB"] = "Nintendo_-_Game_Boy", ["GBC"] = "Nintendo_-_Game_Boy_Color", ["GBA"] = "Nintendo_-_Game_Boy_Advance", ["NDS"] = "Nintendo_-_Nintendo_DS",
        ["N3DS"] = "Nintendo_-_Nintendo_3DS", ["VIRTUAL_BOY"] = "Nintendo_-_Virtual_Boy", ["GAMECUBE"] = "Nintendo_-_GameCube", ["WII"] = "Nintendo_-_Wii",
        ["WII_U"] = "Nintendo_-_Wii_U", ["PS1"] = "Sony_-_PlayStation", ["PS2"] = "Sony_-_PlayStation_2", ["PS3"] = "Sony_-_PlayStation_3",
        ["PS4"] = "Sony_-_PlayStation_4", ["PSP"] = "Sony_-_PlayStation_Portable", ["XBOX"] = "Microsoft_-_Xbox", ["XBOX_360"] = "Microsoft_-_Xbox_360",
        ["GENESIS"] = "Sega_-_Mega_Drive_-_Genesis", ["SEGA_32X"] = "Sega_-_32X", ["GAME_GEAR"] = "Sega_-_Game_Gear", ["SATURN"] = "Sega_-_Saturn",
        ["DREAMCAST"] = "Sega_-_Dreamcast", ["CDI"] = "Philips_-_CD-i", ["AMIGA"] = "Commodore_-_Amiga", ["ARCADE"] = "FBNeo_-_Arcade_Games",
    };

    // Preferred regions, best first.
    private static readonly string[] Regions = ["USA", "World", "USA, Europe", "Europe", "Brazil", "Japan"];

    public static async Task<int> Run(string catalogDir)
    {
        using var api = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
        api.DefaultRequestHeaders.UserAgent.ParseAdd("LotusPorts-CatalogScout/0.1");
        api.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        if (Environment.GetEnvironmentVariable("GITHUB_TOKEN") is { Length: > 0 } token) api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var gameFiles = Directory.GetFiles(Path.Combine(catalogDir, "games"), "*.json");
        var games = gameFiles.Select(f => (File: f, Node: JsonNode.Parse(File.ReadAllText(f))!.AsObject())).ToList();
        var lists = new Dictionary<string, (string Branch, List<string> Files)>();
        var unavailable = new HashSet<string>();
        int matched = 0, tried = 0;

        foreach (var platform in games.Select(g => g.Node["originalPlatform"]!.GetValue<string>()).Distinct())
        {
            if (!Repos.TryGetValue(platform, out var repo)) continue;
            try
            {
                var info = JsonNode.Parse(await api.GetStringAsync($"repos/libretro-thumbnails/{repo}"))!;
                var branch = info["default_branch"]!.GetValue<string>();
                var root = JsonNode.Parse(await api.GetStringAsync($"repos/libretro-thumbnails/{repo}/git/trees/{branch}"))!;
                var boxSha = root["tree"]!.AsArray().FirstOrDefault(t => t!["path"]!.GetValue<string>() == "Named_Boxarts")?["sha"]?.GetValue<string>();
                if (boxSha is null) continue;
                var box = JsonNode.Parse(await api.GetStringAsync($"repos/libretro-thumbnails/{repo}/git/trees/{boxSha}"))!;
                lists[platform] = (branch, box["tree"]!.AsArray().Select(t => t!["path"]!.GetValue<string>()).Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToList());
                Console.Error.WriteLine($"{repo}: {lists[platform].Files.Count} box arts");
            }
            catch (HttpRequestException ex)
            {
                // Not reachable now (rate limit, network): games of this console keep the cover they have.
                unavailable.Add(platform);
                Console.Error.WriteLine($"{repo}: indisponível ({ex.StatusCode}); capas atuais mantidas");
            }
        }

        // SteamGridDB first (uniform 600x900 portrait art), libretro box art as the fallback.
        var sgdb = await SteamGridDbCoversAsync(catalogDir, games.Select(g => (g.Node["id"]!.GetValue<string>(), g.Node["title"]!.GetValue<string>())).ToList());
        int fromSgdb = 0;

        foreach (var (file, game) in games)
        {
            var platform = game["originalPlatform"]!.GetValue<string>();
            var id = game["id"]!.GetValue<string>();
            JsonObject? cover = null;
            if (sgdb.TryGetValue(id, out var grid) && grid is not null)
            {
                cover = new JsonObject { ["url"] = grid, ["source"] = "steamgriddb" };
                fromSgdb++;
            }
            else if (lists.TryGetValue(platform, out var list))
            {
                tried++;
                var wanted = Key(game["title"]!.GetValue<string>());
                var best = list.Files
                    .Select(f => (File: f, Key: Key(StripTags(Path.GetFileNameWithoutExtension(f))), Region: RegionRank(f)))
                    .Where(x => x.Key == wanted)
                    .OrderBy(x => x.Region).ThenBy(x => x.File.Length)
                    .FirstOrDefault();
                if (best.File is not null)
                {
                    cover = new JsonObject
                    {
                        ["url"] = $"https://raw.githubusercontent.com/libretro-thumbnails/{Repos[platform]}/{list.Branch}/Named_Boxarts/{Uri.EscapeDataString(best.File)}",
                        ["source"] = "libretro-thumbnails",
                    };
                    matched++;
                }
            }
            if (cover is not null) game["cover"] = cover;
            else if (!unavailable.Contains(platform)) game.Remove("cover");
            File.WriteAllText(file, game.ToJsonString(Pretty) + "\n");
        }
        var total = games.Count(g => g.Node["cover"] is not null);
        Console.Error.WriteLine($"covers: {total} of {games.Count} games ({fromSgdb} SteamGridDB, {matched} libretro of {tried} tried)");
        return Generator.Bundle(catalogDir);
    }

    /// <summary>
    /// Game id → SteamGridDB grid address (or null when there is no exact match). Results are kept in
    /// research/steamgriddb.json so a rerun only asks about games not checked yet.
    /// </summary>
    private static async Task<Dictionary<string, string?>> SteamGridDbCoversAsync(string catalogDir, List<(string Id, string Title)> games)
    {
        var result = new Dictionary<string, string?>();
        var cacheFile = Path.Combine(catalogDir, "research", "steamgriddb.json");
        var cache = File.Exists(cacheFile) ? JsonNode.Parse(File.ReadAllText(cacheFile))!.AsObject() : [];
        foreach (var (id, entry) in cache) result[id] = entry?["grid"]?.GetValue<string>();

        if (SteamGridDb.ReadKey() is not { } key)
        {
            Console.Error.WriteLine("SteamGridDB: sem chave (SGDB_API_KEY ou %USERPROFILE%\\.lotusports\\steamgriddb.key); usando só o que já foi consultado.");
            return result;
        }
        using var http = SteamGridDb.Client(key);
        var todo = games.Where(g => !cache.ContainsKey(g.Id)).ToList();
        Console.Error.WriteLine($"SteamGridDB: {todo.Count} jogos para consultar ({cache.Count} já consultados)");
        var done = 0;
        foreach (var (id, title) in todo)
        {
            var found = await SteamGridDb.FindAsync(http, title, Key);
            string? grid = found is { } f ? await SteamGridDb.BestGridAsync(http, f.Id) : null;
            cache[id] = new JsonObject
            {
                ["sgdbId"] = found?.Id,
                ["name"] = found?.Name,
                ["grid"] = grid,
                ["checkedAt"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            };
            result[id] = grid;
            if (++done % 25 == 0)
            {
                File.WriteAllText(cacheFile, cache.ToJsonString(Pretty) + "\n");
                Console.Error.WriteLine($"  {done}/{todo.Count}");
            }
            await Task.Delay(250); // gentle with a free community API
        }
        File.WriteAllText(cacheFile, cache.ToJsonString(Pretty) + "\n");
        return result;
    }

    private static int RegionRank(string file)
    {
        var tags = TagRegex().Matches(file).Select(m => m.Groups[1].Value).ToList();
        for (var i = 0; i < Regions.Length; i++) if (tags.Any(t => t == Regions[i])) return i;
        return Regions.Length;
    }

    private static string StripTags(string name) => TagRegex().Replace(name, "").Trim();

    /// <summary>"Legend of Zelda, The - Majora's Mask" and "The Legend of Zelda: Majora's Mask" give the same key.</summary>
    private static string Key(string title)
    {
        var parts = title.Split(" - ").Select(p => ArticleRegex().Replace(p.Trim(), m => $"{m.Groups[2].Value} {m.Groups[1].Value}")).ToArray();
        var s = string.Join(" ", parts).Replace("&", " and ", StringComparison.Ordinal);
        var normalized = s.Normalize(NormalizationForm.FormD);
        var b = new StringBuilder();
        foreach (var c in normalized) if (char.IsAsciiLetterOrDigit(c)) b.Append(char.ToLowerInvariant(c));
        return b.ToString();
    }

    [GeneratedRegex(@"\s*\(([^)]*)\)|\s*\[[^\]]*\]")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"^(.*), (The|A|An)$", RegexOptions.IgnoreCase)]
    private static partial Regex ArticleRegex();
}
