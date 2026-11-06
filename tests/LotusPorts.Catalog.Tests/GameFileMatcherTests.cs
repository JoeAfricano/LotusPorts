using LotusPorts.Core.GameFiles;

namespace LotusPorts.Catalog.Tests;

public class GameFileMatcherTests
{
    private static readonly GameFileHashes Hashes = new(
        Sha1: new string('a', 40), Md5: new string('b', 32), Sha256: new string('c', 64), Crc32: "12345678");

    private static AcceptedVersion Version(string? sha1 = null, string? md5 = null, string? sha256 = null, string? crc = null, string version = "1.0") =>
        new("EUA", version, null, sha1, md5, sha256, crc, "https://github.com/o/r#readme");

    [Fact]
    public void Strongest_matching_hash_wins_and_case_does_not_matter()
    {
        GameFileMatcher.MatchedBy(Version(sha1: new string('A', 40)), Hashes).Should().Be("SHA-1");
        GameFileMatcher.MatchedBy(Version(sha256: new string('c', 64), sha1: new string('a', 40)), Hashes).Should().Be("SHA-256");
        GameFileMatcher.MatchedBy(Version(md5: new string('b', 32)), Hashes).Should().Be("MD5");
    }

    [Fact]
    public void A_different_hash_or_a_placeholder_never_matches()
    {
        GameFileMatcher.MatchedBy(Version(sha1: new string('f', 40)), Hashes).Should().BeNull();
        GameFileMatcher.MatchedBy(Version(sha1: "PREENCHER_SHA1"), Hashes).Should().BeNull();
        GameFileMatcher.MatchedBy(Version(), Hashes).Should().BeNull();
    }

    [Fact]
    public void Crc32_counts_only_when_nothing_stronger_was_recorded()
    {
        GameFileMatcher.MatchedBy(Version(crc: "12345678"), Hashes).Should().Be("CRC32");
        GameFileMatcher.MatchedBy(Version(crc: "12345678", sha1: new string('f', 40)), Hashes).Should().BeNull();
    }

    [Fact]
    public void A_file_with_a_known_hash_in_the_real_catalog_is_recognized_for_its_game()
    {
        var bundle = CatalogValidator.Validate(CatalogValidator.ReadResource("catalog.json")).Bundle!;
        var library = CatalogLibrary.Build([new("official", "Oficial", CatalogOrigin.Official, true, bundle)]);
        var (port, version) = bundle.Ports.SelectMany(p => p.RequiredGameFiles.SelectMany(f => f.AcceptedVersions).Select(v => (p, v)))
            .First(x => x.v.Sha1 is { Length: 40 } s && s.All(Uri.IsHexDigit));

        var matches = GameFileMatcher.Match(library.Games, Hashes with { Sha1 = version.Sha1!.ToUpperInvariant() });

        matches.Should().Contain(m => m.Port.Id == port.Id && m.Game.Id == port.GameId && m.MatchedBy == "SHA-1");
    }
}

public class GuideTranslationTests
{
    [Fact]
    public void Every_translation_in_the_catalog_is_embedded_with_its_source_and_fingerprint()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LotusPorts.slnx"))) dir = dir.Parent;
        var files = Directory.GetFiles(Path.Combine(dir!.FullName, "catalog", "guides", "pt-BR"), "*.md");
        files.Should().NotBeEmpty();
        foreach (var file in files)
        {
            var translation = GuideTranslations.Find(Path.GetFileNameWithoutExtension(file), "pt-BR");
            translation.Should().NotBeNull(file);
            translation!.Source.Host.Should().Be("github.com");
            translation.Markdown.Should().NotBeNullOrWhiteSpace();
        }
        GuideTranslations.Find("does-not-exist", "pt-BR").Should().BeNull();
    }
}
