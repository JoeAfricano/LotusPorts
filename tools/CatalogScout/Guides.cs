using System.Net;
using System.Text.Json.Nodes;
using LotusPorts.Core.Guides;

/// <summary>
/// Exports the "how to play" sections of the recommended ports' READMEs (English), exactly as the app extracts them,
/// to catalog/research/guides-en/{portId}.md, and reports which Portuguese translations in catalog/guides/pt-BR are
/// missing or outdated. Each translation starts with a header line recording the fingerprint of the English it came from.
/// </summary>
internal static class Guides
{
    private static readonly string[] Names = ["README.md", "readme.md", "Readme.md", "README.MD"];

    public static async Task<int> Run(string catalogDir)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("LotusPorts-CatalogScout/0.1");
        var outDir = Path.Combine(catalogDir, "research", "guides-en");
        var ptDir = Path.Combine(catalogDir, "guides", "pt-BR");
        Directory.CreateDirectory(outDir);
        Directory.CreateDirectory(ptDir);

        var ports = Directory.GetFiles(Path.Combine(catalogDir, "ports"), "*.json")
            .Select(f => JsonNode.Parse(File.ReadAllText(f))!.AsObject()).ToDictionary(p => p["id"]!.GetValue<string>());
        var recommended = Directory.GetFiles(Path.Combine(catalogDir, "games"), "*.json")
            .Select(f => JsonNode.Parse(File.ReadAllText(f))!)
            .Where(g => g["review"]?["state"]?.GetValue<string>() == "approved")
            .Select(g => g["recommendedPortId"]!.GetValue<string>())
            .Where(ports.ContainsKey).Distinct().Order().ToList();

        int exported = 0, portuguese = 0, none = 0, missing = 0, outdated = 0;
        var exportedIds = new HashSet<string>();
        foreach (var id in recommended)
        {
            var owner = ports[id]["repo"]!["owner"]!.GetValue<string>();
            var name = ports[id]["repo"]!["name"]!.GetValue<string>();
            string? markdown = null, file = null;
            foreach (var candidate in Names)
            {
                using var response = await http.GetAsync($"https://raw.githubusercontent.com/{owner}/{name}/HEAD/{candidate}");
                if (response.StatusCode == HttpStatusCode.NotFound) continue;
                if (!response.IsSuccessStatusCode) break;
                markdown = await response.Content.ReadAsStringAsync();
                file = candidate;
                break;
            }
            if (markdown is null) { none++; continue; }
            var guide = ReadmeGuide.Extract(markdown, new Uri($"https://github.com/{owner}/{name}/blob/HEAD/"), "pt-BR");
            if (guide is null) { none++; continue; }
            if (guide.Portuguese) { portuguese++; continue; }

            var fingerprint = ReadmeGuide.Fingerprint(guide);
            var source = $"https://github.com/{owner}/{name}/blob/HEAD/{file}";
            File.WriteAllText(Path.Combine(outDir, id + ".md"), $"<!-- source: {source} | fingerprint: {fingerprint} -->\n\n{ReadmeGuide.ToMarkdown(guide)}");
            exportedIds.Add(id);
            exported++;

            var pt = Path.Combine(ptDir, id + ".md");
            if (!File.Exists(pt)) { missing++; Console.WriteLine($"  missing  {id}"); }
            else if (!File.ReadLines(pt).First().Contains($"fingerprint: {fingerprint}", StringComparison.Ordinal)) { outdated++; Console.WriteLine($"  outdated {id}"); }
        }
        // English exports for ports that no longer have a guide are removed.
        foreach (var stale in Directory.GetFiles(outDir, "*.md").Where(f => !exportedIds.Contains(Path.GetFileNameWithoutExtension(f)))) File.Delete(stale);

        Console.Error.WriteLine($"guides: {recommended.Count} recommended ports; {exported} English guides exported, {portuguese} already in Portuguese, {none} without a usage section");
        Console.Error.WriteLine($"pt-BR translations: {missing} missing, {outdated} outdated");
        return 0;
    }
}
