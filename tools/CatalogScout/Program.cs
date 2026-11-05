// CatalogScout: gathers evidence about port repositories for catalog curation.
// Reads public GitHub data only (repo info, releases, README) and lists the files inside official
// Windows release zips by reading just the zip index (HTTP range request). Nothing is installed.
//
// Usage: dotnet run --project tools/CatalogScout -- <repos.txt> <out.json>
// Token: GITHUB_TOKEN env var (optional; raises the API limit). Never written anywhere.

using System.Buffers.Binary;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

// Commands:
//   scan <repos.txt> <scan.json>                     gather evidence from GitHub
//   generate <seeds.json> <scan.json> <catalogDir> [--force]   write games/ports files (keeps manual edits unless --force)
//   bundle <catalogDir>                              rebuild catalog.json from games/ and ports/
if (args.Length > 0 && args[0] == "generate") return Generator.Generate(args[1], args[2], args[3], args.Contains("--force"));
if (args.Length > 0 && args[0] == "bundle") return Generator.Bundle(args[1]);
//   approve <catalogDir> <reviewer> --all | <gameId>...   mark games as reviewed (they become public)
if (args.Length > 0 && args[0] == "approve") return Generator.Approve(args[1], args[2], args[3..]);
if (args.Length > 0 && args[0] == "scan") args = args[1..];
if (args.Length < 2) { Console.Error.WriteLine("usage: CatalogScout scan <repos.txt> <out.json> | generate ... | bundle ..."); return 2; }
var repos = File.ReadAllLines(args[0]).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#')).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

using var api = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
api.DefaultRequestHeaders.UserAgent.ParseAdd("LotusPorts-CatalogScout/0.1");
api.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
if (!string.IsNullOrWhiteSpace(token)) api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
using var files = new HttpClient();
files.DefaultRequestHeaders.UserAgent.ParseAdd("LotusPorts-CatalogScout/0.1");

var windows = new Regex(@"(?i)(win|windows|x64|x86_64|\.exe$)");
var notWindows = new Regex(@"(?i)(linux|mac|osx|darwin|appimage|flatpak|android|switch|arm64|\.apk$|\.dmg$|debug|pdb|symbols|source)");
var hashLine = new Regex(@"(?i)\b([0-9a-f]{40}|[0-9a-f]{64}|[0-9a-f]{32})\b");
var pathLine = new Regex(@"(?i)(%appdata%|%localappdata%|appdata|portable\.txt|saves? (are|is|file)|config(uration)? (file|folder)|\.ini\b|\.json\b|\.cfg\b)");

var setupLine = new Regex(@"(?i)((select|provide|choose|drag|drop|place|put|copy|point)\b.{0,60}\b(rom|copy|disc|iso|xex|dump|game files|baserom|data))|(first (launch|run|start))|(when prompted)");

var results = new JsonArray();
foreach (var repo in repos)
{
    Console.Error.WriteLine($"scan {repo}");
    var entry = new JsonObject { ["repo"] = repo };
    try
    {
        var info = await GetJson($"repos/{repo}");
        if (info is null) { entry["error"] = "not found"; results.Add(entry); continue; }
        entry["fullName"] = info["full_name"]?.GetValue<string>();
        entry["description"] = info["description"]?.GetValue<string>();
        entry["archived"] = info["archived"]?.GetValue<bool>();
        entry["fork"] = info["fork"]?.GetValue<bool>();
        entry["license"] = info["license"]?["spdx_id"]?.GetValue<string>();
        entry["stars"] = info["stargazers_count"]?.GetValue<int>();
        entry["homepage"] = info["homepage"]?.GetValue<string>();
        entry["pushedAt"] = info["pushed_at"]?.GetValue<string>();
        var fullName = entry["fullName"]!.GetValue<string>();

        var releases = (await GetJson($"repos/{fullName}/releases?per_page=15")) as JsonArray ?? [];
        JsonObject? chosen = null;
        foreach (var preferStable in new[] { true, false })
        {
            chosen = releases.OfType<JsonObject>().FirstOrDefault(r =>
                r["draft"]?.GetValue<bool>() != true &&
                (r["prerelease"]?.GetValue<bool>() != true) == preferStable &&
                WindowsAssets(r).Any());
            if (chosen is not null) break;
        }
        entry["releaseCount"] = releases.Count;
        if (chosen is not null)
        {
            var assets = WindowsAssets(chosen).ToList();
            var release = new JsonObject
            {
                ["tag"] = chosen["tag_name"]?.GetValue<string>(),
                ["name"] = chosen["name"]?.GetValue<string>(),
                ["publishedAt"] = chosen["published_at"]?.GetValue<string>(),
                ["prerelease"] = chosen["prerelease"]?.GetValue<bool>(),
                ["windowsAssets"] = new JsonArray(assets.Select(a => (JsonNode)new JsonObject
                {
                    ["name"] = a["name"]!.GetValue<string>(),
                    ["size"] = a["size"]!.GetValue<long>(),
                    ["digest"] = a["digest"]?.GetValue<string>(),
                }).ToArray()),
            };
            var zips = new JsonObject();
            foreach (var zip in assets.Where(a => a["name"]!.GetValue<string>().EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).Take(4))
            {
                var listing = new JsonObject();
                try
                {
                    var names = await ZipEntries(zip["browser_download_url"]!.GetValue<string>(), zip["size"]!.GetValue<long>());
                    listing["entryCount"] = names.Count;
                    listing["executables"] = new JsonArray(names.Where(n => n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).Select(n => (JsonNode)n.Replace('\\', '/')).ToArray());
                    listing["topLevel"] = new JsonArray(names.Select(n => n.Replace('\\', '/').Split('/')[0]).Distinct().Take(25).Select(n => (JsonNode)n).ToArray());
                }
                catch (Exception ex) { listing["error"] = ex.Message; }
                zips[zip["name"]!.GetValue<string>()] = listing;
            }
            release["zips"] = zips;
            entry["release"] = release;
        }

        var readme = await GetText($"repos/{fullName}/readme");
        if (readme is not null)
        {
            var lines = readme.Split('\n').Select(l => l.Trim()).ToList();
            entry["readmeHashLines"] = new JsonArray(lines.Where(l => hashLine.IsMatch(l)).Take(20).Select(l => (JsonNode)Cut(l)).ToArray());
            entry["readmePathLines"] = new JsonArray(lines.Where(l => pathLine.IsMatch(l)).Take(12).Select(l => (JsonNode)Cut(l)).ToArray());
            entry["readmeSetupLines"] = new JsonArray(lines.Where(l => setupLine.IsMatch(l)).Take(8).Select(l => (JsonNode)Cut(l)).ToArray());
        }
    }
    catch (Exception ex) { entry["error"] = ex.Message; }
    results.Add(entry);
}

File.WriteAllText(args[1], results.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
Console.Error.WriteLine($"done: {results.Count} repos");
return 0;

IEnumerable<JsonObject> WindowsAssets(JsonNode release) =>
    (release["assets"] as JsonArray ?? []).OfType<JsonObject>().Where(a =>
    {
        var n = a["name"]!.GetValue<string>();
        return windows.IsMatch(n) && !notWindows.IsMatch(n);
    });

async Task<JsonNode?> GetJson(string path)
{
    using var response = await api.GetAsync(path);
    if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
    response.EnsureSuccessStatusCode();
    return JsonNode.Parse(await response.Content.ReadAsStringAsync());
}

async Task<string?> GetText(string path)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, path);
    request.Headers.Accept.Clear();
    request.Headers.Accept.ParseAdd("application/vnd.github.raw+json");
    using var response = await api.SendAsync(request);
    return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null;
}

async Task<byte[]> Range(string url, long from, long to)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, url);
    request.Headers.Range = new RangeHeaderValue(from, to);
    using var response = await files.SendAsync(request);
    response.EnsureSuccessStatusCode();
    if (response.StatusCode != System.Net.HttpStatusCode.PartialContent) throw new InvalidOperationException("server ignored range request");
    return await response.Content.ReadAsByteArrayAsync();
}

// Reads only the zip central directory (end of file) to list file names.
async Task<List<string>> ZipEntries(string url, long size)
{
    var tailLength = Math.Min(size, 65_557);
    var tail = await Range(url, size - tailLength, size - 1);
    var eocd = -1;
    for (var i = tail.Length - 22; i >= 0; i--)
        if (BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(i)) == 0x06054b50) { eocd = i; break; }
    if (eocd < 0) throw new InvalidOperationException("zip end record not found");
    var cdSize = BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocd + 12));
    var cdOffset = BinaryPrimitives.ReadUInt32LittleEndian(tail.AsSpan(eocd + 16));
    if (cdOffset == 0xFFFFFFFF || cdSize == 0xFFFFFFFF) throw new InvalidOperationException("zip64 not supported by scout");
    var cd = cdOffset >= size - tailLength ? tail.AsSpan((int)(cdOffset - (size - tailLength)), (int)cdSize).ToArray() : await Range(url, cdOffset, cdOffset + cdSize - 1);
    var names = new List<string>();
    for (var p = 0; p + 46 <= cd.Length && BinaryPrimitives.ReadUInt32LittleEndian(cd.AsSpan(p)) == 0x02014b50;)
    {
        int nameLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 28));
        int extraLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 30));
        int commentLen = BinaryPrimitives.ReadUInt16LittleEndian(cd.AsSpan(p + 32));
        names.Add(Encoding.UTF8.GetString(cd, p + 46, nameLen));
        p += 46 + nameLen + extraLen + commentLen;
    }
    return names;
}

static string Cut(string s) => s.Length > 260 ? s[..260] + "…" : s;
