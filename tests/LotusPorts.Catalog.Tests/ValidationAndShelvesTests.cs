using System.Text.Json.Nodes;

namespace LotusPorts.Catalog.Tests;

public class ValidationAndShelvesTests
{
    /// <summary>A minimal valid bundle with one game and one port, edited per test.</summary>
    private static JsonObject Sample(string status = "NAO_AVALIADO", string review = "approved", string gameId = "sample-game", string portId = "sample-port") => JsonNode.Parse($$"""
        {
          "schemaVersion": 1, "name": "Teste", "version": "2026-11-05",
          "games": [{
            "schemaVersion": 1, "id": "{{gameId}}", "title": "Sample Game", "originalPlatform": "N64", "genre": ["aventura"],
            "descriptionPtBr": "Jogo de teste.", "recommendedPortId": "{{portId}}", "alternatives": [],
            "lotusGameKey": "port:{{gameId}}", "review": { "state": "{{review}}" }
          }],
          "ports": [{
            "schemaVersion": 1, "id": "{{portId}}", "gameId": "{{gameId}}", "title": "Sample Port", "portType": "decomp-port", "status": "{{status}}",
            "repo": { "owner": "owner", "name": "repo" }, "windowsAsset": { "pattern": "(?i)windows\\.zip$", "packageType": "zip" },
            "setup": { "kind": null }, "requiredGameFiles": [], "persistentPaths": [],
            "experiencePresets": { "original": { "values": {}, "verified": false }, "remasterizado": { "values": {}, "verified": false } },
            "links": { "repo": "https://github.com/owner/repo", "docs": "https://github.com/owner/repo#readme" }
          }]
        }
        """)!.AsObject();

    private static CatalogValidationResult Validate(JsonNode node) => CatalogValidator.Validate(node.ToJsonString());

    private static JsonObject Port(JsonObject bundle) => bundle["ports"]![0]!.AsObject();

    [Fact]
    public void Sample_is_valid() => Validate(Sample()).IsValid.Should().BeTrue();

    [Fact]
    public void Invalid_json_is_rejected() => CatalogValidator.Validate("{ not json").IsValid.Should().BeFalse();

    [Fact]
    public void Missing_required_field_is_rejected_by_schema()
    {
        var bundle = Sample();
        Port(bundle).Remove("windowsAsset");
        var result = Validate(bundle);
        result.IsValid.Should().BeFalse();
        result.Issues.Should().Contain(i => i.Code == CatalogIssueCodes.Schema);
    }

    [Fact]
    public void Unknown_status_is_rejected_by_schema()
    {
        var bundle = Sample();
        Port(bundle)["status"] = "FUNCIONA_100";
        Validate(bundle).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://raw.githubusercontent.com/libretro-thumbnails/Nintendo_-_Nintendo_64/master/Named_Boxarts/Sample.png", "libretro-thumbnails", true)]
    [InlineData("https://cdn2.steamgriddb.com/grid/0123abcd.png", "steamgriddb", true)]
    [InlineData("https://cdn2.steamgriddb.com/grid/0123abcd.jpg", "steamgriddb", true)]
    [InlineData("https://www.steamgriddb.com/api/v2/grids/game/1", "steamgriddb", false)]
    [InlineData("https://example.com/cover.png", "steamgriddb", false)]
    [InlineData("https://cdn2.steamgriddb.com/grid/0123abcd.webp", "steamgriddb", false)]
    [InlineData("https://cdn2.steamgriddb.com/grid/0123abcd.png", "anywhere", false)]
    public void Cover_accepts_only_known_image_hosts(string url, string source, bool valid)
    {
        var bundle = Sample();
        bundle["games"]![0]!["cover"] = new JsonObject { ["url"] = url, ["source"] = source };
        Validate(bundle).IsValid.Should().Be(valid);
    }

    [Fact]
    public void Broken_references_are_rejected()
    {
        var bundle = Sample();
        bundle["games"]![0]!["recommendedPortId"] = "does-not-exist";
        Validate(bundle).Issues.Should().Contain(i => i.Code == CatalogIssueCodes.MissingReference);
    }

    [Fact]
    public void Links_outside_github_or_declared_docs_are_rejected()
    {
        var bundle = Sample();
        Port(bundle)["requiredGameFiles"] = JsonNode.Parse("""
            [{ "label": "Cartucho", "kind": "single-file", "acceptedVersions": [
              { "region": "EUA", "version": "1.0", "source": "https://random-site.example/hashes" } ] }]
            """);
        Validate(bundle).Issues.Should().Contain(i => i.Code == CatalogIssueCodes.ForbiddenHost);
    }

    [Fact]
    public void Invalid_asset_regex_is_rejected()
    {
        var bundle = Sample();
        Port(bundle)["windowsAsset"]!["pattern"] = "(unclosed";
        Validate(bundle).Issues.Should().Contain(i => i.Code == CatalogIssueCodes.InvalidPattern);
    }

    [Theory]
    [InlineData("romUrl", "https://github.com/owner/repo/releases/download/v1/game.z64")]
    [InlineData("isoDownload", "https://github.com/owner/repo")]
    [InlineData("biosUrl", "https://github.com/owner/repo")]
    [InlineData("keys", "anything")]
    public void Imported_source_with_game_file_field_is_rejected(string field, string value)
    {
        var bundle = Sample();
        Port(bundle)[field] = value;
        var result = Validate(bundle);
        result.IsValid.Should().BeFalse();
        result.ReferencesGameFiles.Should().BeTrue();
    }

    [Theory]
    [InlineData("https://example.com/files/zelda.z64")]
    [InlineData("https://github.com/owner/repo/raw/main/game.iso")]
    [InlineData("https://cdn.example/x/bios.bin?dl=1")]
    public void Any_url_to_a_game_file_is_rejected(string url)
    {
        var bundle = Sample();
        Port(bundle)["links"]!["community"] = url;
        Validate(bundle).ReferencesGameFiles.Should().BeTrue();
    }

    private static CatalogBundle Bundle(JsonObject json) => Validate(json).Bundle!;

    private static GameUserState Nothing(string _) => new(false, false, false, null, null, false);

    [Fact]
    public void Games_still_being_built_appear_only_in_soon()
    {
        var built = Bundle(Sample("EM_DESENVOLVIMENTO", gameId: "building", portId: "building-port"));
        var playable = Bundle(Sample("JOGAVEL", gameId: "playable", portId: "playable-port"));
        var library = CatalogLibrary.Build([
            new("a", "A", CatalogOrigin.Official, true, built),
            new("b", "B", CatalogOrigin.External, false, playable)]);
        var now = DateTimeOffset.UtcNow;

        var shelves = ShelfBuilder.Build(library.Visible(false),
            id => new GameUserState(false, true, true, now, now, false), now.AddDays(-1));

        shelves.Single(s => s.Kind == ShelfKind.Soon).Games.Select(g => g.Id).Should().Equal("building");
        shelves.Where(s => s.Kind != ShelfKind.Soon).SelectMany(s => s.Games).Select(g => g.Id).Should().NotContain("building");
    }

    [Fact]
    public void Nothing_is_new_on_the_first_catalog_load()
    {
        var library = CatalogLibrary.Build([new("a", "A", CatalogOrigin.Official, true, Bundle(Sample()))]);
        var now = DateTimeOffset.UtcNow;
        ShelfBuilder.Build(library.Visible(false), id => new GameUserState(false, false, false, null, now, false), newSince: null)
            .Should().NotContain(s => s.Kind == ShelfKind.New);
    }

    [Fact]
    public void Pending_games_only_show_in_review_mode()
    {
        var library = CatalogLibrary.Build([new("a", "A", CatalogOrigin.Official, true, Bundle(Sample(review: "pending")))]);
        library.Visible(showInReview: false).Should().BeEmpty();
        library.Visible(showInReview: true).Should().ContainSingle().Which.Visibility.Should().Be(GameVisibility.InReview);
    }

    [Fact]
    public void Official_wins_conflicts_and_external_port_becomes_alternative()
    {
        var official = Bundle(Sample(gameId: "same-game", portId: "official-port"));
        var external = Bundle(Sample(gameId: "same-game", portId: "external-port"));
        var library = CatalogLibrary.Build([
            new("ext", "Fonte X", CatalogOrigin.External, false, external),
            new("off", "Oficial", CatalogOrigin.Official, true, official)]);

        var game = library.Games.Should().ContainSingle().Subject;
        game.Origin.Should().Be(CatalogOrigin.Official);
        game.Recommended.Port.Id.Should().Be("official-port");
        game.Alternatives.Should().ContainSingle().Which.Should().Match<PortChoice>(c => c.Port.Id == "external-port" && c.Origin == CatalogOrigin.External && !c.Trusted);
    }

    [Fact]
    public void External_port_cannot_redefine_an_official_port_id()
    {
        var official = Bundle(Sample(gameId: "game-a", portId: "shared-port"));
        var external = Bundle(Sample(gameId: "game-b", portId: "shared-port"));
        var library = CatalogLibrary.Build([
            new("off", "Oficial", CatalogOrigin.Official, true, official),
            new("ext", "Fonte X", CatalogOrigin.External, false, external)]);

        library.Games.Select(g => g.Id).Should().Equal("game-a");
        library.Warnings.Should().ContainSingle(w => w.SourceName == "Fonte X");
    }

    [Fact]
    public void External_games_are_flagged_until_trusted()
    {
        var library = CatalogLibrary.Build([new("ext", "Fonte X", CatalogOrigin.External, false, Bundle(Sample(review: "pending")))]);
        var game = library.Visible(false).Should().ContainSingle().Subject;
        game.Visibility.Should().Be(GameVisibility.External);
        game.Trusted.Should().BeFalse();
    }

    [Fact]
    public void Tabs_and_chips_filter()
    {
        var a = Bundle(Sample(gameId: "game-a", portId: "port-a"));
        var b = Bundle(Sample(gameId: "game-b", portId: "port-b"));
        b.Games[0].Should().NotBeNull();
        var library = CatalogLibrary.Build([new("a", "A", CatalogOrigin.Official, true, a), new("b", "B", CatalogOrigin.Official, true, b)]);
        GameUserState State(string id) => new(id == "game-a", false, false, null, null, false);

        ShelfBuilder.Apply(library.Games, new LibraryFilter(LibraryTab.Favorites), State).Select(g => g.Id).Should().Equal("game-a");
        ShelfBuilder.Apply(library.Games, new LibraryFilter(Platform: Platform.PS1), State).Should().BeEmpty();
        ShelfBuilder.Apply(library.Games, new LibraryFilter(Genre: "AVENTURA"), Nothing).Should().HaveCount(2);
    }
}
