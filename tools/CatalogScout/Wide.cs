using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>
/// Builds catalog entries for many ports at once from a discovery map (repo -> game facts) plus scan evidence.
/// Only facts are taken from the map (which game, which console, genre, the Windows file name pattern);
/// every release, file and executable is checked against the scan. Descriptions are our own text.
/// Entries that cannot be resolved without guessing are skipped and reported, never invented.
/// </summary>
internal static partial class Wide
{
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static readonly Dictionary<string, string> Platforms = new()
    {
        ["n64"] = "N64", ["x360"] = "XBOX_360", ["xbox"] = "XBOX", ["ps1"] = "PS1", ["ps2"] = "PS2", ["ps3"] = "PS3", ["ps4"] = "PS4", ["ps5"] = "PS5",
        ["gc"] = "GAMECUBE", ["wii"] = "WII", ["wiiu"] = "WII_U", ["snes"] = "SNES", ["gba"] = "GBA", ["gb"] = "GB", ["gbc"] = "GBC", ["ds"] = "NDS",
        ["3ds"] = "N3DS", ["dc"] = "DREAMCAST", ["saturn"] = "SATURN", ["psp"] = "PSP", ["switch"] = "SWITCH", ["nes"] = "NES", ["genesis"] = "GENESIS",
        ["s32x"] = "SEGA_32X", ["gg"] = "GAME_GEAR", ["arcade"] = "ARCADE", ["cdi"] = "CDI", ["amiga"] = "AMIGA", ["vb"] = "VIRTUAL_BOY", ["other"] = "OTHER",
    };

    private static readonly Dictionary<string, string> PlatformNames = new()
    {
        ["N64"] = "Nintendo 64", ["XBOX_360"] = "Xbox 360", ["XBOX"] = "Xbox", ["PS1"] = "PlayStation", ["PS2"] = "PlayStation 2", ["PS3"] = "PlayStation 3",
        ["PS4"] = "PlayStation 4", ["PS5"] = "PlayStation 5", ["GAMECUBE"] = "GameCube", ["WII"] = "Wii", ["WII_U"] = "Wii U", ["SNES"] = "Super Nintendo",
        ["GBA"] = "Game Boy Advance", ["GB"] = "Game Boy", ["GBC"] = "Game Boy Color", ["NDS"] = "Nintendo DS", ["N3DS"] = "Nintendo 3DS", ["DREAMCAST"] = "Dreamcast",
        ["SATURN"] = "Saturn", ["PSP"] = "PSP", ["SWITCH"] = "Nintendo Switch", ["NES"] = "NES", ["GENESIS"] = "Mega Drive", ["SEGA_32X"] = "32X",
        ["GAME_GEAR"] = "Game Gear", ["ARCADE"] = "Arcade", ["CDI"] = "CD-i", ["AMIGA"] = "Amiga", ["VIRTUAL_BOY"] = "Virtual Boy", ["OTHER"] = "outra plataforma",
    };

    private static readonly Dictionary<string, string> Genres = new()
    {
        ["action"] = "ação", ["platformer"] = "plataforma", ["fighting"] = "luta", ["rpg"] = "RPG", ["racing"] = "corrida",
        ["shooter"] = "tiro", ["adventure"] = "aventura", ["sports"] = "esporte", ["puzzle"] = "quebra-cabeça",
    };

    /// <summary>Repositories kept out on purpose (no legal copy of the game exists, or not a game).</summary>
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
    {
        "DinosaurPlanetRecomp/dino-recomp", "arcanite24/pinyon-shift", "StonedModder/auto-tmc", "open-goal/jak-project",
    };

    // Programs inside a release that are never the game itself.
    [GeneratedRegex(@"(?i)(updater|update\.exe|crashpad|crash_?handler|createdump|unins|uninstall|vc_?redist|dxsetup|directx|setup|install|ffmpeg|python|pythonw|7z|7za|extract|dotnet|helper|clang|lld|llvm|gcc|g\+\+|cc1|collect2|lto-wrapper|\bas\.exe|\bld\.exe|editor|compiler|server|dedicated|identify|preview|settings\.exe|config\.exe|benchmark|test)")]
    private static partial Regex HelperExe();

    [GeneratedRegex(@"v?\d+(?:[._]\d+)+(?:[-_.]?(?:alpha|beta|rc|pre|dev|hotfix)[-_.]?\d*)?|\d{6,}")]
    private static partial Regex VersionLike();

    public static int Generate(string mapPath, string[] scanPaths, string catalogDir)
    {
        var map = JsonNode.Parse(File.ReadAllText(mapPath))!["entries"]!.AsArray().OfType<JsonObject>()
            .GroupBy(e => e["repo"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var scan = scanPaths.SelectMany(p => JsonNode.Parse(File.ReadAllText(p))!.AsArray().OfType<JsonObject>()).ToList();

        var gamesDir = Path.Combine(catalogDir, "games");
        var portsDir = Path.Combine(catalogDir, "ports");
        var existingGames = Directory.GetFiles(gamesDir, "*.json").ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => JsonNode.Parse(File.ReadAllText(f))!.AsObject());
        var existingPorts = Directory.GetFiles(portsDir, "*.json").Select(f => JsonNode.Parse(File.ReadAllText(f))!.AsObject()).ToList();
        var knownRepos = existingPorts.Select(p => $"{p["repo"]!["owner"]}/{p["repo"]!["name"]}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var portIds = existingPorts.Select(p => p["id"]!.GetValue<string>()).ToHashSet();

        // A discovered game that matches one we already have attaches to it (found through a shared repository).
        var gameByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in existingPorts)
            if (map.TryGetValue($"{p["repo"]!["owner"]}/{p["repo"]!["name"]}", out var m))
                gameByName[Norm(m["game"]!.GetValue<string>())] = p["gameId"]!.GetValue<string>();
        foreach (var (id, g) in existingGames) gameByName.TryAdd(Norm(g["title"]!.GetValue<string>()), id);

        var skipped = new Dictionary<string, List<string>>();
        void Skip(string reason, string repo) => (skipped.TryGetValue(reason, out var l) ? l : skipped[reason] = []).Add(repo);

        var newPorts = new List<(JsonObject Port, string GameKey, int Stars, JsonObject Facts)>();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        foreach (var s in scan)
        {
            var repo = s["fullName"]?.GetValue<string>() ?? s["repo"]!.GetValue<string>();
            if (Excluded.Contains(repo)) { Skip("excluído de propósito", repo); continue; }
            if (knownRepos.Contains(repo)) continue;
            if (s["release"] is not JsonObject release) { Skip("sem release para Windows", repo); continue; }
            if (s["archived"]?.GetValue<bool>() == true) { Skip("repositório arquivado", repo); continue; }
            if (!map.TryGetValue(repo, out var facts) && !map.TryGetValue(s["repo"]!.GetValue<string>(), out facts)) { Skip("jogo não identificado", repo); continue; }

            var assets = release["windowsAssets"]!.AsArray().OfType<JsonObject>().Select(a => a["name"]!.GetValue<string>()).ToList();
            var asset = ChooseAsset(assets, facts["windows"]?.GetValue<string>());
            if (asset is null) { Skip("arquivo para Windows ambíguo", repo); continue; }
            var pattern = PatternFor(asset);
            if (assets.Count(a => Regex.IsMatch(a, pattern)) != 1) pattern = "(?i)^" + Regex.Escape(asset) + "$";

            var ext = Path.GetExtension(asset).ToLowerInvariant();
            var package = ext switch
            {
                ".zip" => "zip",
                ".7z" => "7z",
                ".msi" => "installer",
                ".exe" when Regex.IsMatch(asset, "(?i)(setup|install)") => "installer",
                ".exe" => "portable-exe",
                _ => null,
            };
            if (package is null) { Skip("formato de pacote não suportado", repo); continue; }

            string? exe = package switch
            {
                "portable-exe" => asset,
                "zip" => ChooseExe(release["zips"]?[asset]?["executables"]?.AsArray().Select(x => x!.GetValue<string>()).ToList() ?? [], facts["app"]!.GetValue<string>(), repo),
                _ => null,
            };

            var owner = repo.Split('/')[0];
            var name = repo.Split('/')[1];
            var portId = UniqueId(Slug(facts["app"]!.GetValue<string>()), owner, portIds);
            var docs = $"https://github.com/{owner}/{name}#readme";
            var digest = s["release"]!["windowsAssets"]!.AsArray().OfType<JsonObject>().First(a => a["name"]!.GetValue<string>() == asset)["digest"]?.GetValue<string>();
            var size = s["release"]!["windowsAssets"]!.AsArray().OfType<JsonObject>().First(a => a["name"]!.GetValue<string>() == asset)["size"]?.GetValue<long>();
            var port = new JsonObject
            {
                ["schemaVersion"] = 1,
                ["id"] = portId,
                ["gameId"] = "",
                ["title"] = facts["app"]!.GetValue<string>(),
                ["portType"] = Regex.IsMatch(repo + " " + facts["app"], "(?i)recomp") ? "static-recomp" : "decomp-port",
                ["status"] = "NAO_AVALIADO",
                ["knownIssuesPtBr"] = new JsonArray(),
                ["verifiedBy"] = null,
                ["verifiedAt"] = null,
                ["repo"] = new JsonObject { ["owner"] = owner, ["name"] = name },
                ["license"] = s["license"]?.GetValue<string>() is { } lic && lic != "NOASSERTION" ? lic : null,
                ["windowsAsset"] = new JsonObject
                {
                    ["pattern"] = pattern,
                    ["excludePattern"] = null,
                    ["preferPrerelease"] = release["prerelease"]?.GetValue<bool>() ?? false,
                    ["packageType"] = package,
                },
                ["executable"] = exe,
                ["launchArgs"] = new JsonArray(),
                ["setup"] = new JsonObject { ["kind"] = null, ["targetFileName"] = null, ["targetFolder"] = null, ["notesPtBr"] = null, ["extractCommand"] = null, ["expectedOutput"] = null, ["requiredEntries"] = null },
                ["requiredGameFiles"] = new JsonArray(),
                ["persistentPaths"] = new JsonArray(),
                ["experiencePresets"] = new JsonObject
                {
                    ["configFile"] = null,
                    ["configFormat"] = null,
                    ["original"] = new JsonObject { ["values"] = new JsonObject(), ["verified"] = false },
                    ["remasterizado"] = new JsonObject { ["values"] = new JsonObject(), ["verified"] = false },
                },
                ["dependencies"] = null,
                ["graphicsApis"] = null,
                ["controllerSupport"] = null,
                ["highlightsPtBr"] = new JsonArray(),
                ["links"] = new JsonObject { ["repo"] = $"https://github.com/{owner}/{name}", ["docs"] = docs, ["community"] = null },
                ["art"] = new JsonObject { ["icon"] = null, ["banner"] = null, ["artLicense"] = null },
                ["minWindowsBuild"] = null,
                ["evidence"] = new JsonObject
                {
                    ["scannedAt"] = today,
                    ["releaseTag"] = release["tag"]?.GetValue<string>(),
                    ["releaseDate"] = DateOnly(release["publishedAt"]?.GetValue<string>()),
                    ["stars"] = s["stars"]?.GetValue<int>(),
                    ["prerelease"] = release["prerelease"]?.GetValue<bool>() ?? false,
                    ["assetName"] = asset,
                    ["assetSize"] = size,
                    ["assetSha256"] = digest?.StartsWith("sha256:", StringComparison.Ordinal) == true ? digest[7..] : null,
                },
            };
            newPorts.Add((port, Norm(facts["game"]!.GetValue<string>()), s["stars"]?.GetValue<int>() ?? 0, facts));
        }

        var addedGames = 0;
        var attached = 0;
        var gameIds = existingGames.Keys.ToHashSet();
        foreach (var group in newPorts.GroupBy(p => p.GameKey))
        {
            var ordered = group.OrderByDescending(p => p.Stars).ToList();
            if (gameByName.TryGetValue(group.Key, out var existingId))
            {
                // Same game we already list: the new ports become other ways to play it.
                var file = Path.Combine(gamesDir, existingId + ".json");
                var game = JsonNode.Parse(File.ReadAllText(file))!.AsObject();
                foreach (var p in ordered)
                {
                    p.Port["gameId"] = existingId;
                    game["alternatives"]!.AsArray().Add(new JsonObject { ["portId"] = p.Port["id"]!.GetValue<string>(), ["differencePtBr"] = null });
                    Write(Path.Combine(portsDir, p.Port["id"] + ".json"), p.Port);
                    attached++;
                }
                Write(file, game);
                continue;
            }

            var facts = ordered[0].Facts;
            var title = facts["game"]!.GetValue<string>().Trim();
            var gameId = UniqueId(Slug(title), "jogo", gameIds);
            var platform = Platforms.GetValueOrDefault(facts["origin"]?.GetValue<string>() ?? "other", "OTHER");
            var genre = Genres.TryGetValue(facts["genre"]?.GetValue<string>() ?? "", out var g) ? new JsonArray(g) : new JsonArray();
            foreach (var p in ordered) p.Port["gameId"] = gameId;
            var newGame = new JsonObject
            {
                ["schemaVersion"] = 1,
                ["id"] = gameId,
                ["title"] = title,
                ["originalPlatform"] = platform,
                ["releaseYear"] = null,
                ["genre"] = genre,
                ["descriptionPtBr"] = $"{title}, lançado originalmente para {PlatformNames[platform]}, em versão nativa para PC feita pela comunidade. Você joga com a sua própria cópia do jogo.",
                ["recommendedPortId"] = ordered[0].Port["id"]!.GetValue<string>(),
                ["recommendationReasonPtBr"] = ordered.Count > 1 ? "Proposta: o port mais acompanhado no GitHub para este jogo." : null,
                ["alternatives"] = new JsonArray(ordered.Skip(1).Select(p => (JsonNode)new JsonObject { ["portId"] = p.Port["id"]!.GetValue<string>(), ["differencePtBr"] = null }).ToArray()),
                ["lotusGameKey"] = "port:" + gameId,
                ["review"] = new JsonObject { ["state"] = "pending", ["by"] = null, ["at"] = null, ["notesPtBr"] = "Adicionado automaticamente a partir de dados conferidos no GitHub; aguardando a revisão do curador." },
            };
            Write(Path.Combine(gamesDir, gameId + ".json"), newGame);
            foreach (var p in ordered) Write(Path.Combine(portsDir, p.Port["id"] + ".json"), p.Port);
            addedGames++;
        }

        Console.Error.WriteLine($"new games {addedGames}, new ports {newPorts.Count} ({attached} attached to existing games), no executable chosen: {newPorts.Count(p => p.Port["executable"] is null)}");
        foreach (var (reason, repos) in skipped.OrderByDescending(s => s.Value.Count))
            Console.Error.WriteLine($"skipped ({reason}): {repos.Count}");
        File.WriteAllText(Path.Combine(catalogDir, "research", "wide-skipped.json"), JsonSerializer.Serialize(skipped, Pretty));
        return Generator.Bundle(catalogDir);
    }

    /// <summary>The Windows download: the discovery glob when it matches exactly one file, else the only Windows file, else a 64-bit zip.</summary>
    private static string? ChooseAsset(List<string> assets, string? glob)
    {
        if (assets.Count == 0) return null;
        if (!string.IsNullOrEmpty(glob))
        {
            var rx = "(?i)^" + Regex.Escape(glob).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            var hits = assets.Where(a => Regex.IsMatch(a, rx)).ToList();
            if (hits.Count == 1) return hits[0];
        }
        if (assets.Count == 1) return assets[0];
        var preferred = assets.Where(a => Regex.IsMatch(a, @"(?i)(x64|x86_64|win64|amd64)") && !Regex.IsMatch(a, @"(?i)(noavx|debug|symbols|pdb|lsp|dev)")).ToList();
        return preferred.Count == 1 ? preferred[0] : null;
    }

    /// <summary>A pattern that keeps matching future releases: version numbers become wildcards.</summary>
    private static string PatternFor(string asset) =>
        "(?i)^" + string.Join(".*", VersionLike().Split(asset).Select(Regex.Escape)) + "$";

    private static string? ChooseExe(List<string> exes, string app, string repo)
    {
        var candidates = exes.Where(e => !HelperExe().IsMatch(Path.GetFileName(e))).ToList();
        if (candidates.Count == 1) return candidates[0];
        if (candidates.Count == 0) return null;
        // Several programs: take the one named like the project, only if exactly one is.
        var keys = new[] { app, repo.Split('/')[1] }.Select(k => Regex.Replace(k.ToLowerInvariant(), "[^a-z0-9]", "")).Where(k => k.Length >= 3).ToList();
        var named = candidates.Where(e =>
        {
            var n = Regex.Replace(Path.GetFileNameWithoutExtension(e).ToLowerInvariant(), "[^a-z0-9]", "");
            return keys.Any(k => k.Contains(n) || n.Contains(k));
        }).ToList();
        if (named.Count == 1) return named[0];
        var shallow = candidates.Where(e => !e.Contains('/')).ToList();
        return shallow.Count == 1 ? shallow[0] : null;
    }

    internal static string? DateOnly(string? iso) => iso is { Length: >= 10 } ? iso[..10] : null;

    /// <summary>Refreshes the measured facts (release date, stars) of ports already in the catalog, by repository.</summary>
    public static int RefreshEvidence(string catalogDir, string[] scanPaths)
    {
        var scan = scanPaths.SelectMany(p => JsonNode.Parse(File.ReadAllText(p))!.AsArray().OfType<JsonObject>())
            .Where(s => s["fullName"] is not null)
            .GroupBy(s => s["fullName"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var updated = 0;
        foreach (var file in Directory.GetFiles(Path.Combine(catalogDir, "ports"), "*.json"))
        {
            var port = JsonNode.Parse(File.ReadAllText(file))!.AsObject();
            if (!scan.TryGetValue($"{port["repo"]!["owner"]}/{port["repo"]!["name"]}", out var s)) continue;
            var evidence = port["evidence"] as JsonObject ?? new JsonObject();
            var fresh = new JsonObject();
            foreach (var (key, value) in evidence)
            {
                fresh[key] = value?.DeepClone();
                if (key == "releaseTag")
                {
                    fresh["releaseDate"] = s["release"]?["tag"]?.GetValue<string>() == value?.GetValue<string>() ? DateOnly(s["release"]?["publishedAt"]?.GetValue<string>()) : null;
                    fresh["stars"] = s["stars"]?.GetValue<int>();
                }
            }
            port["evidence"] = fresh;
            Write(file, port);
            updated++;
        }
        Console.Error.WriteLine($"evidence refreshed: {updated} ports");
        return Generator.Bundle(catalogDir);
    }

    private static string Norm(string s) => Regex.Replace(Slug(s), "-+", "-");

    internal static string Slug(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var b = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            b.Append(char.IsAsciiLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }
        var slug = Regex.Replace(b.ToString(), "-+", "-").Trim('-');
        if (slug.Length > 70) slug = slug[..70].Trim('-');
        return slug.Length == 0 ? "item" : slug;
    }

    private static string UniqueId(string id, string suffix, HashSet<string> used)
    {
        var candidate = id;
        if (used.Contains(candidate)) candidate = $"{id}-{Slug(suffix)}";
        for (var i = 2; used.Contains(candidate); i++) candidate = $"{id}-{i}";
        used.Add(candidate);
        return candidate;
    }

    private static void Write(string path, JsonObject node) => File.WriteAllText(path, node.ToJsonString(Pretty) + "\n");
}
