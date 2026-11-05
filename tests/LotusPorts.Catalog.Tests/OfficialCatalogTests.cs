using System.Text.Json.Nodes;

namespace LotusPorts.Catalog.Tests;

/// <summary>The catalog shipped with the app, and the per-file sources it is built from.</summary>
public class OfficialCatalogTests
{
    private static string CatalogDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LotusPorts.slnx"))) dir = dir.Parent;
        return Path.Combine(dir?.FullName ?? throw new InvalidOperationException("Repository root not found."), "catalog");
    }

    [Fact]
    public void Embedded_catalog_is_valid()
    {
        var result = CatalogValidator.Validate(CatalogValidator.ReadResource("catalog.json"));

        result.Issues.Where(i => i.Severity == IssueSeverity.Error).Should().BeEmpty();
        result.IsValid.Should().BeTrue();
        result.Bundle!.Games.Should().NotBeEmpty();
    }

    [Fact]
    public void Every_game_and_port_file_is_in_the_bundle_unchanged()
    {
        var dir = CatalogDir();
        var bundle = JsonNode.Parse(File.ReadAllText(Path.Combine(dir, "catalog.json")))!;
        foreach (var (folder, key) in new[] { ("games", "games"), ("ports", "ports") })
        {
            var files = Directory.GetFiles(Path.Combine(dir, folder), "*.json").Select(f => JsonNode.Parse(File.ReadAllText(f))!).ToList();
            var bundled = bundle[key]!.AsArray().ToList();
            bundled.Should().HaveCount(files.Count, $"catalog.json must be rebuilt after editing {folder}/ (CatalogScout bundle)");
            foreach (var file in files)
            {
                var id = file["id"]!.GetValue<string>();
                var match = bundled.Single(b => b!["id"]!.GetValue<string>() == id);
                JsonNode.DeepEquals(file, match).Should().BeTrue($"{folder}/{id}.json differs from catalog.json");
            }
        }
    }

    [Fact]
    public void Every_file_name_matches_its_id()
    {
        var dir = CatalogDir();
        foreach (var folder in new[] { "games", "ports" })
            foreach (var file in Directory.GetFiles(Path.Combine(dir, folder), "*.json"))
                JsonNode.Parse(File.ReadAllText(file))!["id"]!.GetValue<string>().Should().Be(Path.GetFileNameWithoutExtension(file));
    }

    [Fact]
    public void Nothing_is_public_before_review()
    {
        var bundle = CatalogValidator.Validate(CatalogValidator.ReadResource("catalog.json")).Bundle!;
        var library = CatalogLibrary.Build([new CatalogSourceBundle("official", "Oficial", CatalogOrigin.Official, true, bundle)]);

        foreach (var game in library.Games)
            (game.Visibility == GameVisibility.Public).Should().Be(game.Game.Review.State == ReviewState.Approved, game.Id);
    }

    [Fact]
    public void Experience_styles_are_never_shown_unverified()
    {
        var bundle = CatalogValidator.Validate(CatalogValidator.ReadResource("catalog.json")).Bundle!;
        bundle.Ports.Where(p => p.ExperiencePresets.IsVerified).Should().AllSatisfy(p =>
            p.ExperiencePresets.Original.Values.Should().NotBeEmpty());
    }
}
