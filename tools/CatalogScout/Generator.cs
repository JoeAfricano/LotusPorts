using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

/// <summary>Turns curated seeds plus scan evidence into catalog files. Fails loudly instead of guessing.</summary>
internal static class Generator
{
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static int Generate(string seedsPath, string scanPath, string catalogDir, bool force)
    {
        var seeds = JsonNode.Parse(File.ReadAllText(seedsPath))!["entries"]!.AsArray().OfType<JsonObject>().ToList();
        var scan = JsonNode.Parse(File.ReadAllText(scanPath))!.AsArray().OfType<JsonObject>().ToList();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var errors = new List<string>();
        var games = new Dictionary<string, JsonObject>();
        var ports = new List<JsonObject>();

        foreach (var seed in seeds)
        {
            var repo = seed["repo"]!.GetValue<string>();
            var p = seed["port"]!.AsObject();
            var portId = p["id"]!.GetValue<string>();
            var evidence = scan.FirstOrDefault(s =>
                string.Equals(s["repo"]?.GetValue<string>(), repo, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s["fullName"]?.GetValue<string>(), repo, StringComparison.OrdinalIgnoreCase));
            var release = evidence?["release"] as JsonObject;
            if (evidence is null || release is null) { errors.Add($"{portId}: no Windows release in scan for {repo}"); continue; }

            var fullName = evidence["fullName"]!.GetValue<string>();
            var owner = fullName.Split('/')[0];
            var name = fullName.Split('/')[1];
            var assetSeed = p["asset"]?.AsObject() ?? new JsonObject();
            var pattern = assetSeed["pattern"]?.GetValue<string>() ?? throw new InvalidOperationException($"{portId}: asset pattern required");
            var exclude = assetSeed["exclude"]?.GetValue<string>();
            var assets = release["windowsAssets"]!.AsArray().OfType<JsonObject>().ToList();
            var matches = assets.Where(a =>
            {
                var n = a["name"]!.GetValue<string>();
                return Regex.IsMatch(n, pattern) && (exclude is null || !Regex.IsMatch(n, exclude));
            }).ToList();
            if (matches.Count != 1) { errors.Add($"{portId}: pattern {pattern} matches {matches.Count} assets ({string.Join(", ", assets.Select(a => a["name"]))})"); continue; }
            var asset = matches[0];
            var assetName = asset["name"]!.GetValue<string>();
            var package = assetSeed["package"]?.GetValue<string>() ?? (assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "zip" : "portable-exe");

            string? exe = p["exe"]?.GetValue<string>();
            var listing = release["zips"]?[assetName] as JsonObject;
            var exes = listing?["executables"]?.AsArray().Select(x => x!.GetValue<string>()).ToList() ?? [];
            if (package == "zip")
            {
                if (exe is null)
                {
                    if (exes.Count == 1) exe = exes[0];
                    else if (exes.Count > 1) { errors.Add($"{portId}: several executables, choose one: {string.Join(", ", exes)}"); continue; }
                }
                else if (exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && !exes.Contains(exe))
                {
                    errors.Add($"{portId}: executable {exe} not found in {assetName} ({string.Join(", ", exes)})"); continue;
                }
            }
            else if (package == "portable-exe") exe = assetName;

            var digest = asset["digest"]?.GetValue<string>();
            var docs = $"https://github.com/{owner}/{name}#readme";
            var setup = p["setup"]?.AsObject();
            var port = new JsonObject
            {
                ["schemaVersion"] = 1,
                ["id"] = portId,
                ["gameId"] = seed["game"]?["id"]?.GetValue<string>() ?? seed["gameId"]!.GetValue<string>(),
                ["title"] = p["title"]!.GetValue<string>(),
                ["portType"] = p["type"]!.GetValue<string>(),
                ["status"] = "NAO_AVALIADO",
                ["knownIssuesPtBr"] = new JsonArray(),
                ["verifiedBy"] = null,
                ["verifiedAt"] = null,
                ["repo"] = new JsonObject { ["owner"] = owner, ["name"] = name },
                ["license"] = evidence["license"]?.GetValue<string>() is { } lic && lic != "NOASSERTION" ? lic : null,
                ["windowsAsset"] = new JsonObject
                {
                    ["pattern"] = pattern,
                    ["excludePattern"] = exclude,
                    // A port that only publishes test versions must say so, or no download would ever match.
                    ["preferPrerelease"] = assetSeed["prerelease"]?.GetValue<bool>() ?? release["prerelease"]?.GetValue<bool>() ?? false,
                    ["packageType"] = package,
                },
                ["executable"] = exe,
                ["launchArgs"] = new JsonArray(),
                ["setup"] = new JsonObject
                {
                    ["kind"] = setup?["kind"]?.GetValue<string>(),
                    ["targetFileName"] = setup?["file"]?.GetValue<string>(),
                    ["targetFolder"] = setup?["folder"]?.GetValue<string>(),
                    ["notesPtBr"] = setup?["notes"]?.GetValue<string>(),
                    ["extractCommand"] = null,
                    ["expectedOutput"] = null,
                    ["requiredEntries"] = null,
                },
                ["requiredGameFiles"] = new JsonArray((p["files"]?.AsArray() ?? []).OfType<JsonObject>().Select(f => (JsonNode)new JsonObject
                {
                    ["label"] = f["label"]!.GetValue<string>(),
                    ["kind"] = f["kind"]!.GetValue<string>(),
                    ["format"] = f["format"]?.GetValue<string>(),
                    ["canonicalFormat"] = f["canonical"]?.GetValue<string>(),
                    ["acceptedVersions"] = new JsonArray((f["versions"]?.AsArray() ?? []).OfType<JsonObject>().Select(v => (JsonNode)new JsonObject
                    {
                        ["region"] = v["region"]!.GetValue<string>(),
                        ["version"] = v["version"]!.GetValue<string>(),
                        ["fileName"] = v["fileName"]?.GetValue<string>(),
                        ["sha1"] = v["sha1"]?.GetValue<string>(),
                        ["md5"] = v["md5"]?.GetValue<string>(),
                        ["sha256"] = v["sha256"]?.GetValue<string>(),
                        ["crc32"] = v["crc32"]?.GetValue<string>(),
                        ["source"] = docs,
                    }).ToArray()),
                    ["officialDocsUrl"] = docs,
                }).ToArray()),
                ["persistentPaths"] = p["paths"]?.DeepClone() ?? new JsonArray(),
                ["experiencePresets"] = new JsonObject
                {
                    ["configFile"] = null,
                    ["configFormat"] = null,
                    ["original"] = new JsonObject { ["values"] = new JsonObject(), ["verified"] = false },
                    ["remasterizado"] = new JsonObject { ["values"] = new JsonObject(), ["verified"] = false },
                },
                ["dependencies"] = null,
                ["graphicsApis"] = p["apis"]?.DeepClone(),
                ["controllerSupport"] = null,
                ["highlightsPtBr"] = new JsonArray(),
                ["links"] = new JsonObject { ["repo"] = $"https://github.com/{owner}/{name}", ["docs"] = docs, ["community"] = null },
                ["art"] = new JsonObject { ["icon"] = null, ["banner"] = null, ["artLicense"] = null },
                ["minWindowsBuild"] = null,
                ["evidence"] = new JsonObject
                {
                    ["scannedAt"] = today,
                    ["releaseTag"] = release["tag"]?.GetValue<string>(),
                    ["prerelease"] = release["prerelease"]?.GetValue<bool>() ?? false,
                    ["assetName"] = assetName,
                    ["assetSize"] = asset["size"]?.GetValue<long>(),
                    ["assetSha256"] = digest?.StartsWith("sha256:", StringComparison.Ordinal) == true ? digest["sha256:".Length..] : null,
                },
            };
            ports.Add(port);

            if (seed["game"] is JsonObject g)
            {
                var gameId = g["id"]!.GetValue<string>();
                games[gameId] = new JsonObject
                {
                    ["schemaVersion"] = 1,
                    ["id"] = gameId,
                    ["title"] = g["title"]!.GetValue<string>(),
                    ["originalPlatform"] = g["platform"]!.GetValue<string>(),
                    ["releaseYear"] = null,
                    ["genre"] = g["genre"]!.DeepClone(),
                    ["descriptionPtBr"] = g["desc"]!.GetValue<string>(),
                    ["recommendedPortId"] = portId,
                    ["recommendationReasonPtBr"] = seed["reason"]?.GetValue<string>(),
                    ["alternatives"] = new JsonArray(),
                    ["lotusGameKey"] = "port:" + gameId,
                    ["review"] = new JsonObject
                    {
                        ["state"] = "pending",
                        ["by"] = null,
                        ["at"] = null,
                        ["notesPtBr"] = "Proposta montada a partir dos dados oficiais do repositório; aguardando a revisão do curador.",
                    },
                };
            }
            else
            {
                var gameId = seed["gameId"]!.GetValue<string>();
                if (!games.TryGetValue(gameId, out var game)) { errors.Add($"{portId}: game {gameId} must come before its alternatives"); continue; }
                game["alternatives"]!.AsArray().Add(new JsonObject { ["portId"] = portId, ["differencePtBr"] = seed["difference"]?.GetValue<string>() });
            }
        }

        if (errors.Count > 0)
        {
            foreach (var e in errors) Console.Error.WriteLine("ERROR " + e);
            return 1;
        }

        var written = 0;
        written += WriteAll(Path.Combine(catalogDir, "games"), games.Values, force);
        written += WriteAll(Path.Combine(catalogDir, "ports"), ports, force);
        Console.Error.WriteLine($"games {games.Count}, ports {ports.Count}, files written {written} (existing files kept unless --force)");
        return Bundle(catalogDir);
    }

    /// <summary>Marks games as reviewed by the curator. Only reviewed games are public in the app.</summary>
    public static int Approve(string catalogDir, string reviewer, string[] ids)
    {
        var all = ids.Contains("--all");
        var files = Directory.GetFiles(Path.Combine(catalogDir, "games"), "*.json");
        var wanted = ids.Where(i => i != "--all").ToHashSet();
        var unknown = wanted.Where(id => !files.Any(f => Path.GetFileNameWithoutExtension(f) == id)).ToList();
        if (unknown.Count > 0) { Console.Error.WriteLine("ERROR unknown game ids: " + string.Join(", ", unknown)); return 1; }
        var count = 0;
        foreach (var file in files)
        {
            var game = JsonNode.Parse(File.ReadAllText(file))!.AsObject();
            if (!all && !wanted.Contains(game["id"]!.GetValue<string>())) continue;
            game["review"] = new JsonObject
            {
                ["state"] = "approved",
                ["by"] = reviewer,
                ["at"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ["notesPtBr"] = null,
            };
            File.WriteAllText(file, game.ToJsonString(Pretty) + "\n");
            count++;
        }
        Console.Error.WriteLine($"approved {count} games");
        return Bundle(catalogDir);
    }

    public static int Bundle(string catalogDir)
    {
        JsonArray Read(string folder) => new(Directory.GetFiles(Path.Combine(catalogDir, folder), "*.json").Order(StringComparer.Ordinal)
            .Select(f => JsonNode.Parse(File.ReadAllText(f))!).ToArray());
        var bundle = new JsonObject
        {
            ["$schema"] = "./catalog.schema.json",
            ["schemaVersion"] = 1,
            ["name"] = "Catálogo oficial do Lotus Ports",
            ["version"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["games"] = Read("games"),
            ["ports"] = Read("ports"),
        };
        File.WriteAllText(Path.Combine(catalogDir, "catalog.json"), bundle.ToJsonString(Pretty) + "\n");
        Console.Error.WriteLine($"bundle: {bundle["games"]!.AsArray().Count} games, {bundle["ports"]!.AsArray().Count} ports");
        return 0;
    }

    private static int WriteAll(string folder, IEnumerable<JsonObject> items, bool force)
    {
        Directory.CreateDirectory(folder);
        var count = 0;
        foreach (var item in items)
        {
            var path = Path.Combine(folder, item["id"]!.GetValue<string>() + ".json");
            if (File.Exists(path) && !force) continue;
            File.WriteAllText(path, item.ToJsonString(Pretty) + "\n");
            count++;
        }
        return count;
    }
}
