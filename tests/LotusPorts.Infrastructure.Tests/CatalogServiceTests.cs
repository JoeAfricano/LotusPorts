using System.Net;
using LotusPorts.Infrastructure.Catalog;
using LotusPorts.Infrastructure.Database;
using LotusPorts.Infrastructure.Database.Migrations;
using LotusPorts.Infrastructure.Network;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>Imported catalog sources against a fake GitHub (no network).</summary>
public sealed class CatalogServiceTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));
    private readonly FakeGitHub server = new();
    private readonly CatalogService service;

    public CatalogServiceTests()
    {
        var paths = new AppPaths(folder);
        paths.EnsureStartupFolders();
        var db = new SqliteDatabase(paths.DatabaseFile);
        new MigrationRunner(db, AllMigrations.List, NullLogger<MigrationRunner>.Instance).MigrateAsync().GetAwaiter().GetResult();
        service = new CatalogService(paths, new SqliteCatalogSourceStore(db), new SqliteGameStateStore(db, TimeProvider.System),
            new GitHubClient(new HttpClient(server)), TimeProvider.System, NullLogger<CatalogService>.Instance);
        service.LoadAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private static string Bundle(string gameId, string extraPortField = "") => $$"""
        { "schemaVersion": 1, "name": "Fonte da comunidade", "version": "2026-11-05",
          "games": [{ "schemaVersion": 1, "id": "{{gameId}}", "title": "Jogo {{gameId}}", "originalPlatform": "PC", "genre": [],
            "descriptionPtBr": "Teste.", "recommendedPortId": "{{gameId}}-port", "alternatives": [], "lotusGameKey": "port:{{gameId}}", "review": { "state": "pending" } }],
          "ports": [{ "schemaVersion": 1, "id": "{{gameId}}-port", "gameId": "{{gameId}}", "title": "Port", "portType": "source-port", "status": "JOGAVEL",
            "repo": { "owner": "someone", "name": "port" }, "windowsAsset": { "pattern": "win", "packageType": "zip" }, "setup": { "kind": null },
            "requiredGameFiles": [], "persistentPaths": [], {{extraPortField}}
            "experiencePresets": { "original": { "values": {}, "verified": false }, "remasterizado": { "values": {}, "verified": false } },
            "links": { "repo": "https://github.com/someone/port" } }] }
        """;

    private void ServeRepo(string catalog, string etag = "\"v1\"")
    {
        server.Json("https://api.github.com/repos/someone/catalog", """{ "full_name": "someone/catalog", "default_branch": "main", "archived": false }""");
        server.Text("https://raw.githubusercontent.com/someone/catalog/main/catalog.json", catalog, etag);
    }

    [Fact]
    public async Task Valid_source_is_added_and_its_games_appear_as_external()
    {
        ServeRepo(Bundle("community-game"));

        var result = await service.AddSourceAsync("https://github.com/someone/catalog");

        result.Outcome.Should().Be(AddSourceOutcome.Added);
        var game = service.Library.Find("community-game")!;
        game.Origin.Should().Be(LotusPorts.Catalog.CatalogOrigin.External);
        game.Trusted.Should().BeFalse();
    }

    [Fact]
    public async Task Source_with_game_file_field_is_flagged_and_never_active()
    {
        ServeRepo(Bundle("bad-game", "\"romUrl\": \"https://example.com/game.z64\","));

        var result = await service.AddSourceAsync("https://github.com/someone/catalog");

        result.Outcome.Should().Be(AddSourceOutcome.GameFilesRejected);
        service.Sources.Should().ContainSingle(s => !s.Active && s.ValidationState == SourceValidationState.Warning);
        service.Library.Find("bad-game").Should().BeNull();
    }

    [Fact]
    public async Task Invalid_source_is_rejected_with_reasons()
    {
        ServeRepo("""{ "schemaVersion": 1, "name": "x" }""");
        var result = await service.AddSourceAsync("https://github.com/someone/catalog");
        result.Outcome.Should().Be(AddSourceOutcome.Invalid);
        result.Issues.Should().NotBeEmpty();
        service.Sources.Should().BeEmpty();
    }

    [Theory]
    [InlineData("https://gitlab.com/someone/catalog")]
    [InlineData("http://github.com/someone/catalog")]
    [InlineData("https://github.com/someone")]
    [InlineData("https://example.com/catalog.json")]
    public async Task Only_github_links_are_accepted(string link) =>
        (await service.AddSourceAsync(link)).Outcome.Should().Be(AddSourceOutcome.InvalidLink);

    [Fact]
    public async Task Refresh_uses_etag_and_keeps_copy_when_update_is_invalid()
    {
        ServeRepo(Bundle("community-game"));
        var added = (await service.AddSourceAsync("https://github.com/someone/catalog")).Source!;

        server.Text(added.Url, "", "\"v1\"", notModifiedFor: "\"v1\"");
        (await service.RefreshSourceAsync(added.Id)).ValidationState.Should().Be(SourceValidationState.Ok);
        server.LastIfNoneMatch.Should().Be("\"v1\"");

        server.Text(added.Url, "{ broken", "\"v2\"");
        var refreshed = await service.RefreshSourceAsync(added.Id);
        refreshed.ValidationState.Should().Be(SourceValidationState.Warning);
        service.Library.Find("community-game").Should().NotBeNull("the previous valid copy is kept");
    }

    [Fact]
    public async Task Removing_a_source_removes_its_games()
    {
        ServeRepo(Bundle("community-game"));
        var added = (await service.AddSourceAsync("https://github.com/someone/catalog")).Source!;
        await service.RemoveSourceAsync(added.Id);
        service.Library.Find("community-game").Should().BeNull();
    }

    [Fact]
    public async Task Rate_limit_is_reported_with_reset_time()
    {
        server.RateLimited("https://api.github.com/repos/someone/catalog", DateTimeOffset.FromUnixTimeSeconds(2_000_000_000));
        var result = await service.AddSourceAsync("https://github.com/someone/catalog");
        result.Outcome.Should().Be(AddSourceOutcome.RateLimited);
        result.RetryAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(2_000_000_000));
    }

    [Fact]
    public async Task Official_catalog_loads_and_favorites_persist()
    {
        service.Official.Valid.Should().BeTrue();
        var id = service.Library.Games[0].Id;
        await service.SetFavoriteAsync(id, true);
        service.StateOf(id).Favorite.Should().BeTrue();
        service.NewSince.Should().NotBeNull();
    }

    private sealed class FakeGitHub : HttpMessageHandler
    {
        private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> routes = [];
        public string? LastIfNoneMatch { get; private set; }

        public void Json(string url, string json) => routes[url] = _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };

        public void Text(string url, string body, string etag, string? notModifiedFor = null) => routes[url] = request =>
        {
            LastIfNoneMatch = request.Headers.IfNoneMatch.FirstOrDefault()?.ToString();
            if (notModifiedFor is not null && LastIfNoneMatch == notModifiedFor) return new HttpResponseMessage(HttpStatusCode.NotModified);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue(etag);
            return response;
        };

        public void RateLimited(string url, DateTimeOffset reset) => routes[url] = _ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            response.Headers.Add("X-RateLimit-Remaining", "0");
            response.Headers.Add("X-RateLimit-Reset", reset.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture));
            return response;
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(routes.TryGetValue(request.RequestUri!.GetLeftPart(UriPartial.Query), out var route)
                ? route(request)
                : new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
