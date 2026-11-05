using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Schema;

namespace LotusPorts.Catalog;

public enum IssueSeverity
{
    Warning,
    Error,
}

/// <summary>Why a catalog was rejected. Codes are stable; messages are technical (advanced mode and logs).</summary>
public sealed record CatalogIssue(IssueSeverity Severity, string Code, string Path, string Message);

public sealed record CatalogValidationResult(CatalogBundle? Bundle, IReadOnlyList<CatalogIssue> Issues)
{
    public bool IsValid => Bundle is not null && Issues.All(i => i.Severity != IssueSeverity.Error);

    /// <summary>The catalog tried to reference game files (ROM, ISO, BIOS, keys). Always rejected.</summary>
    public bool ReferencesGameFiles => Issues.Any(i => i.Code == CatalogIssueCodes.GameFileReference);
}

public static class CatalogIssueCodes
{
    public const string InvalidJson = "invalid-json";
    public const string Schema = "schema";
    public const string GameFileReference = "game-file-reference";
    public const string DuplicateId = "duplicate-id";
    public const string MissingReference = "missing-reference";
    public const string WrongGame = "wrong-game";
    public const string ForbiddenHost = "forbidden-host";
    public const string InvalidPattern = "invalid-pattern";
    public const string RepoLinkMismatch = "repo-link-mismatch";
    public const string OrphanPort = "orphan-port";
}

/// <summary>
/// Validates a catalog bundle: JSON Schema (draft 2020-12), then the rules the schema cannot express
/// (cross references, allowed link hosts, and the ban on anything that points to game files).
/// </summary>
public static partial class CatalogValidator
{
    private static readonly Lazy<JsonSchema> Schema = new(() => JsonSchema.FromText(ReadResource("catalog.schema.json")));

    public static string ReadResource(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource {name} not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static CatalogValidationResult Validate(string json)
    {
        var issues = new List<CatalogIssue>();
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { MaxDepth = 32 });
        }
        catch (JsonException ex)
        {
            return new(null, [new(IssueSeverity.Error, CatalogIssueCodes.InvalidJson, "$", ex.Message)]);
        }
        if (root is null) return new(null, [new(IssueSeverity.Error, CatalogIssueCodes.InvalidJson, "$", "Empty document.")]);

        // 1. Anything that looks like a game file reference rejects the whole catalog, before anything else.
        FindGameFileReferences(root, "$", issues);

        // 2. Schema.
        var evaluation = Schema.Value.Evaluate(root, new EvaluationOptions { OutputFormat = OutputFormat.List, RequireFormatValidation = true });
        if (!evaluation.IsValid)
        {
            foreach (var detail in evaluation.Details.Where(d => d.HasErrors).Take(50))
                foreach (var (keyword, message) in detail.Errors!)
                    issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.Schema, detail.InstanceLocation.ToString(), $"{keyword}: {message}"));
        }
        if (issues.Any(i => i.Severity == IssueSeverity.Error)) return new(null, issues);

        CatalogBundle bundle;
        try
        {
            bundle = root.Deserialize<CatalogBundle>(CatalogJson.Options) ?? throw new JsonException("Empty bundle.");
        }
        catch (JsonException ex)
        {
            return new(null, [.. issues, new(IssueSeverity.Error, CatalogIssueCodes.Schema, "$", ex.Message)]);
        }

        // 3. Rules the schema cannot express.
        CheckReferences(bundle, issues);
        foreach (var port in bundle.Ports) CheckPort(port, issues);
        return new(issues.Any(i => i.Severity == IssueSeverity.Error) ? null : bundle, issues);
    }

    private static void CheckReferences(CatalogBundle bundle, List<CatalogIssue> issues)
    {
        foreach (var dup in bundle.Games.GroupBy(g => g.Id).Where(g => g.Count() > 1))
            issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.DuplicateId, $"games/{dup.Key}", "Duplicate game id."));
        foreach (var dup in bundle.Ports.GroupBy(p => p.Id).Where(g => g.Count() > 1))
            issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.DuplicateId, $"ports/{dup.Key}", "Duplicate port id."));

        var ports = bundle.Ports.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        var games = bundle.Games.Select(g => g.Id).ToHashSet();
        var referenced = new HashSet<string>();
        foreach (var game in bundle.Games)
        {
            foreach (var portId in new[] { game.RecommendedPortId }.Concat(game.Alternatives.Select(a => a.PortId)))
            {
                referenced.Add(portId);
                if (!ports.TryGetValue(portId, out var port))
                    issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.MissingReference, $"games/{game.Id}", $"Port {portId} does not exist."));
                else if (port.GameId != game.Id)
                    issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.WrongGame, $"games/{game.Id}", $"Port {portId} belongs to {port.GameId}."));
            }
        }
        foreach (var port in bundle.Ports)
        {
            if (!games.Contains(port.GameId))
                issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.MissingReference, $"ports/{port.Id}", $"Game {port.GameId} does not exist."));
            else if (!referenced.Contains(port.Id))
                issues.Add(new(IssueSeverity.Warning, CatalogIssueCodes.OrphanPort, $"ports/{port.Id}", "Port is not recommended nor listed as an alternative."));
        }
    }

    private static void CheckPort(Port port, List<CatalogIssue> issues)
    {
        var at = $"ports/{port.Id}";
        if (!string.Equals(port.Links.Repo.TrimEnd('/'), port.Repo.Url, StringComparison.OrdinalIgnoreCase))
            issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.RepoLinkMismatch, at, "links.repo must be the declared GitHub repository."));

        foreach (var (field, pattern) in new[] { ("pattern", port.WindowsAsset.Pattern), ("excludePattern", port.WindowsAsset.ExcludePattern) })
        {
            if (pattern is null) continue;
            try { _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100)); }
            catch (ArgumentException ex) { issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.InvalidPattern, $"{at}/windowsAsset/{field}", ex.Message)); }
        }

        // Links may only point to GitHub, or to the docs/community sites the port itself declares as official.
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "github.com", "raw.githubusercontent.com" };
        foreach (var declared in new[] { port.Links.Docs, port.Links.Community })
            if (Uri.TryCreate(declared, UriKind.Absolute, out var u)) allowed.Add(u.Host);

        var urls = new List<(string Path, string? Url)>
        {
            ("links/repo", port.Links.Repo),
            ("art/icon", port.Art?.Icon),
            ("art/banner", port.Art?.Banner),
        };
        foreach (var (file, i) in port.RequiredGameFiles.Select((f, i) => (f, i)))
        {
            urls.Add(($"requiredGameFiles/{i}/officialDocsUrl", file.OfficialDocsUrl));
            urls.AddRange(file.AcceptedVersions.Select((v, j) => ($"requiredGameFiles/{i}/acceptedVersions/{j}/source", (string?)v.Source)));
        }
        foreach (var (path, url) in urls)
        {
            if (url is null) continue;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u) || !allowed.Contains(u.Host))
                issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.ForbiddenHost, $"{at}/{path}", $"Host not allowed: {url}"));
        }
    }

    // Property names that would carry a game file, and URLs that end in a game file extension.
    [GeneratedRegex(@"(?i)^(rom|roms|iso|isos|bios|firmware|keys?|prodkeys|titlekeys|dump|dumps|gamefiles?|disc|discs|wad|nsp|xci)(url|uri|link|links|download|downloads|source|sources|mirror|mirrors)?s?$")]
    private static partial Regex GameFileProperty();

    [GeneratedRegex(@"(?i)\.(z64|n64|v64|sfc|smc|gba|gbc|nds|iso|cue|bin|chd|cso|rvz|gcm|wbfs|wad|wux|wud|xex|nsp|xci|pkg|7z|rar)(\?.*)?$")]
    private static partial Regex GameFileUrl();

    [GeneratedRegex(@"(?i)^(romUrl|isoUrl|biosUrl|keysUrl|gameFileUrl|downloadUrl|mirror)")]
    private static partial Regex DownloadishProperty();

    private static void FindGameFileReferences(JsonNode node, string path, List<CatalogIssue> issues)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var (key, value) in obj)
                {
                    var childPath = $"{path}/{key}";
                    if (GameFileProperty().IsMatch(key) || DownloadishProperty().IsMatch(key))
                        issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.GameFileReference, childPath, $"Field '{key}' is not allowed: catalogs never reference game files."));
                    if (value is not null) FindGameFileReferences(value, childPath, issues);
                }
                break;
            case JsonArray array:
                for (var i = 0; i < array.Count; i++)
                    if (array[i] is { } item) FindGameFileReferences(item, $"{path}/{i}", issues);
                break;
            case JsonValue value when value.TryGetValue<string>(out var text):
                if (Uri.TryCreate(text, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && GameFileUrl().IsMatch(uri.AbsolutePath))
                    issues.Add(new(IssueSeverity.Error, CatalogIssueCodes.GameFileReference, path, $"URL points to a game file: {text}"));
                break;
        }
    }
}
