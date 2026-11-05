using System.Text.Json;
using LotusPorts.Catalog;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Network;
using Microsoft.Extensions.Logging;

namespace LotusPorts.Infrastructure.Catalog;

public enum AddSourceOutcome
{
    Added,
    AlreadyAdded,
    InvalidLink,
    NotFound,
    Invalid,
    GameFilesRejected,
    RateLimited,
    NetworkError,
}

public sealed record AddSourceResult(AddSourceOutcome Outcome, CatalogSourceRecord? Source, IReadOnlyList<CatalogIssue> Issues, DateTimeOffset? RetryAt = null);

public sealed record OfficialCatalogInfo(string Name, string Version, int Games, int Ports, bool Valid, string? Problem);

/// <summary>Candidate Windows download and executables found in a repository's releases ("add port by link").</summary>
public sealed record PortSuggestion(RepositoryInfo Repository, ReleaseInfo Release, ReleaseAssetInfo Asset, IReadOnlyList<string> Executables);

/// <summary>
/// Owns the merged catalog: the official copy shipped with the app, imported sources and the player's own ports.
/// Every catalog is validated before use; an invalid update never replaces a valid copy.
/// </summary>
public sealed class CatalogService(
    AppPaths paths,
    SqliteCatalogSourceStore sources,
    SqliteGameStateStore gameStates,
    GitHubClient github,
    TimeProvider time,
    ILogger<CatalogService> logger)
{
    public const string OfficialSourceId = "official";
    public const string UserSourceId = "user";

    private IReadOnlyDictionary<string, StoredGameState> states = new Dictionary<string, StoredGameState>();

    public event EventHandler? Changed;

    public CatalogLibrary Library { get; private set; } = CatalogLibrary.Empty;
    public OfficialCatalogInfo Official { get; private set; } = new("", "", 0, 0, false, null);
    public IReadOnlyList<CatalogSourceRecord> Sources { get; private set; } = [];
    public CatalogBundle? UserCatalog { get; private set; }

    /// <summary>First catalog load. Games first seen after it are "new" (Chegou!).</summary>
    public DateTimeOffset? NewSince { get; private set; }

    private string SourcesFolder => Path.Combine(paths.Data, "catalog-sources");
    private string UserCatalogFile => Path.Combine(paths.Data, "user-catalog.json");

    public GameUserState StateOf(string gameId) =>
        states.TryGetValue(gameId, out var s)
            ? new GameUserState(s.Favorite, Installed: false, FileRecognized: false, LastPlayed: null, s.FirstSeenAt, s.DismissedFromNew)
            : new GameUserState(false, false, false, null, null, false);

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var bundles = new List<CatalogSourceBundle>();

        var official = CatalogValidator.Validate(CatalogValidator.ReadResource("catalog.json"));
        if (official.IsValid)
        {
            bundles.Add(new(OfficialSourceId, official.Bundle!.Name, CatalogOrigin.Official, true, official.Bundle));
            Official = new(official.Bundle.Name, official.Bundle.Version, official.Bundle.Games.Count, official.Bundle.Ports.Count, true, null);
        }
        else
        {
            // Should never happen: tests validate the embedded copy. Still, never show an unvalidated catalog.
            logger.LogError("Embedded catalog is invalid: {Issues}", string.Join("; ", official.Issues.Take(5).Select(i => $"{i.Path} {i.Message}")));
            Official = new("", "", 0, 0, false, official.Issues.FirstOrDefault()?.Message);
        }

        Sources = await sources.ListAsync(cancellationToken).ConfigureAwait(false);
        foreach (var source in Sources.Where(s => s.Active))
        {
            var cached = ReadCached(source.Id);
            if (cached is not null) bundles.Add(new(source.Id, source.Name, CatalogOrigin.External, source.Trusted, cached));
        }

        UserCatalog = await ReadUserCatalogAsync(cancellationToken).ConfigureAwait(false);
        if (UserCatalog is not null) bundles.Add(new(UserSourceId, UserCatalog.Name, CatalogOrigin.User, true, UserCatalog));

        Library = CatalogLibrary.Build(bundles);
        foreach (var warning in Library.Warnings) logger.LogWarning("Catalog merge ({Source}): {Message}", warning.SourceName, warning.Message);
        NewSince = await gameStates.MarkSeenAsync(Library.Games.Select(g => g.Id), cancellationToken).ConfigureAwait(false);
        states = await gameStates.GetAllAsync(cancellationToken).ConfigureAwait(false);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetFavoriteAsync(string gameId, bool favorite)
    {
        await gameStates.SetFavoriteAsync(gameId, favorite).ConfigureAwait(false);
        states = await gameStates.GetAllAsync().ConfigureAwait(false);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task DismissFromNewAsync(string gameId)
    {
        await gameStates.DismissFromNewAsync(gameId).ConfigureAwait(false);
        states = await gameStates.GetAllAsync().ConfigureAwait(false);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // ---- Imported sources -------------------------------------------------------------------

    public async Task<AddSourceResult> AddSourceAsync(string link, CancellationToken cancellationToken = default)
    {
        if (!SourceAddress.TryParse(link, out var address)) return new(AddSourceOutcome.InvalidLink, null, []);
        try
        {
            var raw = await ResolveCatalogUrlAsync(address!, cancellationToken).ConfigureAwait(false);
            if (raw is null) return new(AddSourceOutcome.NotFound, null, []);
            if (Sources.Any(s => string.Equals(s.Url, raw.ToString(), StringComparison.OrdinalIgnoreCase))) return new(AddSourceOutcome.AlreadyAdded, null, []);

            var fetched = await github.GetTextAsync(raw, null, cancellationToken).ConfigureAwait(false);
            if (fetched.Body is null) return new(AddSourceOutcome.NotFound, null, []);
            var result = CatalogValidator.Validate(fetched.Body);
            var now = time.GetUtcNow();
            var id = "src-" + Guid.NewGuid().ToString("N")[..12];

            if (result.ReferencesGameFiles)
            {
                // Kept in the list with a warning so the player sees why, but never active.
                var flagged = new CatalogSourceRecord(id, raw.ToString(), $"{address!.Owner}/{address.Repo}", false, false, null, now,
                    SourceValidationState.Warning, "Recusada: a fonte tem entradas que apontam para arquivos de jogo.", now);
                await sources.UpsertAsync(flagged, cancellationToken).ConfigureAwait(false);
                await LoadAsync(cancellationToken).ConfigureAwait(false);
                return new(AddSourceOutcome.GameFilesRejected, flagged, result.Issues);
            }
            if (!result.IsValid) return new(AddSourceOutcome.Invalid, null, result.Issues);

            var record = new CatalogSourceRecord(id, raw.ToString(), result.Bundle!.Name, true, false, fetched.ETag, now, SourceValidationState.Ok, null, now);
            WriteCached(id, fetched.Body);
            await sources.UpsertAsync(record, cancellationToken).ConfigureAwait(false);
            await LoadAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Catalog source added: {Url} ({Games} games)", raw, result.Bundle.Games.Count);
            return new(AddSourceOutcome.Added, record, result.Issues);
        }
        catch (GitHubRateLimitException ex) { return new(AddSourceOutcome.RateLimited, null, [], ex.ResetAt); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidDataException or InvalidOperationException)
        {
            logger.LogWarning(ex, "Adding catalog source failed");
            return new(AddSourceOutcome.NetworkError, null, []);
        }
    }

    /// <summary>Checks the source for changes (ETag). An invalid update keeps the previous valid copy.</summary>
    public async Task<CatalogSourceRecord> RefreshSourceAsync(string id, CancellationToken cancellationToken = default)
    {
        var source = Sources.Single(s => s.Id == id);
        CatalogSourceRecord updated;
        try
        {
            var fetched = await github.GetTextAsync(new Uri(source.Url), ReadCached(id) is null ? null : source.ETag, cancellationToken).ConfigureAwait(false);
            var now = time.GetUtcNow();
            if (fetched.NotModified) updated = source with { LastUpdatedAt = now, ValidationState = SourceValidationState.Ok, ValidationMessage = null };
            else if (fetched.Body is null) updated = source with { ValidationState = SourceValidationState.Warning, ValidationMessage = $"A fonte não respondeu (HTTP {(int)fetched.Status}). A cópia anterior foi mantida." };
            else
            {
                var result = CatalogValidator.Validate(fetched.Body);
                if (result.ReferencesGameFiles)
                {
                    DeleteCached(id);
                    updated = source with { Active = false, ValidationState = SourceValidationState.Warning, ValidationMessage = "Recusada: a fonte passou a ter entradas que apontam para arquivos de jogo." };
                }
                else if (!result.IsValid)
                    updated = source with { ValidationState = SourceValidationState.Warning, ValidationMessage = "A nova versão da fonte é inválida. A cópia anterior foi mantida." };
                else
                {
                    WriteCached(id, fetched.Body);
                    updated = source with { Name = result.Bundle!.Name, ETag = fetched.ETag, LastUpdatedAt = now, ValidationState = SourceValidationState.Ok, ValidationMessage = null };
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidDataException or GitHubRateLimitException)
        {
            logger.LogWarning(ex, "Refreshing catalog source {Id} failed", id);
            updated = source with { ValidationState = SourceValidationState.NotVerified, ValidationMessage = "Não foi possível verificar a fonte agora. A cópia anterior foi mantida." };
        }
        await sources.UpsertAsync(updated, cancellationToken).ConfigureAwait(false);
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        return updated;
    }

    /// <summary>A source can only be turned on when a valid copy of it is stored.</summary>
    public Task SetSourceActiveAsync(string id, bool active) => UpdateSource(id, s => s with { Active = active && ReadCached(id) is not null });

    public Task SetSourceTrustedAsync(string id, bool trusted) => UpdateSource(id, s => s with { Trusted = trusted });

    /// <summary>Removes the source from the catalog. Installed games are not touched (the interface says so).</summary>
    public async Task RemoveSourceAsync(string id)
    {
        await sources.DeleteAsync(id).ConfigureAwait(false);
        DeleteCached(id);
        await LoadAsync().ConfigureAwait(false);
    }

    private async Task UpdateSource(string id, Func<CatalogSourceRecord, CatalogSourceRecord> change)
    {
        await sources.UpsertAsync(change(Sources.Single(s => s.Id == id))).ConfigureAwait(false);
        await LoadAsync().ConfigureAwait(false);
    }

    private async Task<Uri?> ResolveCatalogUrlAsync(SourceAddress address, CancellationToken cancellationToken)
    {
        if (address.Kind == SourceAddressKind.RawFile) return address.RawUrl;
        var repo = await github.GetRepositoryAsync(address.Owner, address.Repo, cancellationToken).ConfigureAwait(false);
        return repo is null ? null : new Uri($"https://raw.githubusercontent.com/{repo.Owner}/{repo.Name}/{Uri.EscapeDataString(repo.DefaultBranch)}/catalog.json");
    }

    private CatalogBundle? ReadCached(string id)
    {
        var file = Path.Combine(SourcesFolder, id + ".json");
        if (!File.Exists(file)) return null;
        var result = CatalogValidator.Validate(File.ReadAllText(file));
        if (!result.IsValid) logger.LogWarning("Cached catalog source {Id} is no longer valid; ignored", id);
        return result.IsValid ? result.Bundle : null;
    }

    private void WriteCached(string id, string json)
    {
        Directory.CreateDirectory(SourcesFolder);
        File.WriteAllText(Path.Combine(SourcesFolder, id + ".json"), json);
    }

    private void DeleteCached(string id)
    {
        var file = Path.Combine(SourcesFolder, id + ".json");
        if (File.Exists(file)) File.Delete(file);
    }

    // ---- Ports added by the player ("Adicionado por você") --------------------------------------

    /// <summary>Reads a GitHub repository and suggests the Windows download and executable.</summary>
    public async Task<(PortSuggestion? Suggestion, string? Problem)> InspectRepositoryAsync(string link, CancellationToken cancellationToken = default)
    {
        if (!SourceAddress.TryParse(link, out var address) || address!.Kind != SourceAddressKind.Repository) return (null, "InvalidLink");
        try
        {
            var repo = await github.GetRepositoryAsync(address.Owner, address.Repo, cancellationToken).ConfigureAwait(false);
            if (repo is null) return (null, "NotFound");
            var releases = await github.GetReleasesAsync(repo.Owner, repo.Name, 10, cancellationToken).ConfigureAwait(false);
            foreach (var release in releases.OrderBy(r => r.Prerelease))
            {
                var asset = release.Assets.FirstOrDefault(a => WindowsAssetRules.IsWindowsAsset(a.Name));
                if (asset is null) continue;
                IReadOnlyList<string> exes = [];
                if (asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    try { exes = (await github.ListZipEntriesAsync(asset, cancellationToken).ConfigureAwait(false)).Where(n => n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToList(); }
                    catch (InvalidDataException) { exes = []; }
                }
                else if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) exes = [asset.Name];
                return (new PortSuggestion(repo, release, asset, exes), null);
            }
            return (null, "NoWindowsRelease");
        }
        catch (GitHubRateLimitException) { return (null, "RateLimited"); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Inspecting repository failed");
            return (null, "NetworkError");
        }
    }

    /// <summary>Adds a game and port to the player's own catalog. The whole catalog is validated before saving.</summary>
    public async Task<CatalogValidationResult> AddUserPortAsync(Game game, Port port, CancellationToken cancellationToken = default)
    {
        var current = UserCatalog ?? new CatalogBundle(1, "Adicionados por você", time.GetUtcNow().ToString("yyyy-MM-dd"), [], []);
        var games = current.Games.Where(g => g.Id != game.Id).Append(game).ToList();
        var ports = current.Ports.Where(p => p.Id != port.Id).Append(port).ToList();
        var json = JsonSerializer.Serialize(current with { Version = time.GetUtcNow().ToString("yyyy-MM-dd"), Games = games, Ports = ports }, CatalogJson.Options);
        var result = CatalogValidator.Validate(json);
        if (!result.IsValid) return result;
        Directory.CreateDirectory(paths.Data);
        await File.WriteAllTextAsync(UserCatalogFile, json, cancellationToken).ConfigureAwait(false);
        await LoadAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>The player's ports as a catalog file, for José to include in the official catalog.</summary>
    public string? ExportUserCatalog() => UserCatalog is null ? null : JsonSerializer.Serialize(UserCatalog, CatalogJson.Options);

    private async Task<CatalogBundle?> ReadUserCatalogAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(UserCatalogFile)) return null;
        var result = CatalogValidator.Validate(await File.ReadAllTextAsync(UserCatalogFile, cancellationToken).ConfigureAwait(false));
        if (!result.IsValid) logger.LogWarning("User catalog is invalid and was ignored");
        return result.IsValid ? result.Bundle : null;
    }
}
