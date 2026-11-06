using System.IO.Compression;
using LotusPorts.Catalog;
using LotusPorts.Infrastructure.Install;
using LotusPorts.Infrastructure.Network;

namespace LotusPorts.Infrastructure.Tests;

/// <summary>Zip safety, program detection and release choice.</summary>
public sealed class SafeInstallTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));

    public SafeInstallTests() => Directory.CreateDirectory(folder);

    public void Dispose()
    {
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private string Zip(Action<ZipArchive> fill)
    {
        var file = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".zip");
        using (var zip = ZipFile.Open(file, ZipArchiveMode.Create)) fill(zip);
        return file;
    }

    private static void Add(ZipArchive zip, string name, string content = "x")
    {
        using var writer = new StreamWriter(zip.CreateEntry(name).Open());
        writer.Write(content);
    }

    private string Target => Path.Combine(folder, "out");

    [Theory]
    [InlineData("../evil.txt")]
    [InlineData("game/../../evil.txt")]
    [InlineData("C:/Windows/evil.txt")]
    [InlineData("/evil.txt")]
    public void Entries_cannot_escape_the_folder(string name)
    {
        var zip = Zip(z => { Add(z, "Game.exe"); Add(z, name); });
        var act = () => SafeExtractor.ExtractZip(zip, Target);
        act.Should().Throw<ExtractionException>().Which.Problem.Should().Be(ExtractionProblem.OutsideFolder);
        File.Exists(Path.Combine(folder, "evil.txt")).Should().BeFalse();
    }

    [Fact]
    public void Links_are_skipped()
    {
        var zip = Zip(z =>
        {
            Add(z, "Game.exe");
            var link = z.CreateEntry("link-to-windows");
            link.ExternalAttributes = unchecked((int)(0xA1FFu << 16));
            using var w = new StreamWriter(link.Open());
            w.Write("C:/Windows");
        });
        SafeExtractor.ExtractZip(zip, Target).Should().Be(1);
        File.Exists(Path.Combine(Target, "link-to-windows")).Should().BeFalse();
    }

    [Fact]
    public void Unpacked_size_is_limited()
    {
        var zip = Zip(z => Add(z, "big.bin", new string('a', 10_000)));
        var act = () => SafeExtractor.ExtractZip(zip, Target, new ExtractionLimits(1_000, 10));
        act.Should().Throw<ExtractionException>().Which.Problem.Should().Be(ExtractionProblem.TooLarge);
    }

    [Fact]
    public void A_single_top_folder_is_flattened_but_mixed_tops_are_kept()
    {
        SafeExtractor.ExtractZip(Zip(z => { Add(z, "Game-1.0/Game.exe"); Add(z, "Game-1.0/data/a.txt"); }), Target);
        File.Exists(Path.Combine(Target, "Game.exe")).Should().BeTrue();
        File.Exists(Path.Combine(Target, "data", "a.txt")).Should().BeTrue();

        var other = Path.Combine(folder, "other");
        SafeExtractor.ExtractZip(Zip(z => { Add(z, "bin/Game.exe"); Add(z, "readme.txt"); }), other);
        File.Exists(Path.Combine(other, "bin", "Game.exe")).Should().BeTrue();
    }

    [Fact]
    public void A_file_that_is_not_a_zip_is_reported()
    {
        var file = Path.Combine(folder, "fake.zip");
        File.WriteAllText(file, "<html>not found</html>");
        var act = () => SafeExtractor.ExtractZip(file, Target);
        act.Should().Throw<ExtractionException>().Which.Problem.Should().Be(ExtractionProblem.NotAnArchive);
    }

    private void Exe(string relative, string content = "MZ program")
    {
        var path = Path.Combine(Target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    [Fact]
    public void The_declared_program_is_found_at_the_shallowest_level()
    {
        Exe("Game.exe");
        Exe("tools\\Game.exe");
        Exe("unins000.exe");
        ExecutableFinder.Find(Target, "Game.exe").Should().Be("Game.exe");
        ExecutableFinder.Find(Target, "game.EXE").Should().Be("Game.exe");
        ExecutableFinder.Find(Target, "tools/Game.exe").Should().Be("tools\\Game.exe");
        ExecutableFinder.Find(Target, "Other.exe").Should().BeNull();
    }

    [Fact]
    public void Without_a_declared_program_only_a_single_candidate_counts()
    {
        Exe("Game.exe");
        Exe("unins000.exe");
        Exe("vc_redist.x64.exe");
        ExecutableFinder.Find(Target, null).Should().Be("Game.exe");
        Exe("Editor.exe");
        ExecutableFinder.Find(Target, null).Should().BeNull("two programs: Lotus Ports does not guess");
    }

    [Fact]
    public void A_text_file_named_exe_is_not_a_program()
    {
        Exe("Game.exe", "<html>");
        ExecutableFinder.Find(Target, "Game.exe").Should().BeNull();
    }

    private static ReleaseAssetInfo Asset(string name) => new(name, 10, null, new Uri("https://github.com/o/r/releases/download/t/" + name));

    private static ReleaseInfo Rel(string tag, bool pre, int day, params string[] assets) =>
        new(tag, tag, pre, new DateTimeOffset(2026, 1, day, 0, 0, 0, TimeSpan.Zero), assets.Select(Asset).ToList());

    private static readonly WindowsAsset Rules = new("(?i)windows.*\\.zip$", "(?i)debug", false, "zip");

    [Fact]
    public void The_newest_stable_release_with_one_windows_file_is_chosen()
    {
        var pick = ReleasePicker.Pick([Rel("v3", true, 3, "Game-windows.zip"), Rel("v2", false, 2, "Game-windows.zip", "Game-windows-debug.zip", "Game-linux.zip"), Rel("v1", false, 1, "Game-windows.zip")], Rules);
        pick.Ok.Should().BeTrue();
        pick.Choice!.Release.Tag.Should().Be("v2");
        pick.Choice.Asset.Name.Should().Be("Game-windows.zip");

        ReleasePicker.Pick([Rel("v3", true, 3, "Game-windows.zip")], Rules, allowPrerelease: true).Choice!.Release.Tag.Should().Be("v3");
        ReleasePicker.Pick([Rel("v2", false, 2, "Game-windows.zip"), Rel("v1", false, 1, "Game-windows.zip")], Rules, pinnedTag: "v1").Choice!.Release.Tag.Should().Be("v1");
    }

    [Fact]
    public void Zero_or_several_matches_are_never_guessed()
    {
        ReleasePicker.Pick([Rel("v1", false, 1, "Game-windows-x64.zip", "Game-windows-x86.zip")], Rules).Problem.Should().Be(ReleaseProblem.Ambiguous);
        ReleasePicker.Pick([Rel("v1", false, 1, "Game-linux.zip")], Rules).Problem.Should().Be(ReleaseProblem.NoWindowsFile);
        ReleasePicker.Pick([Rel("v1", true, 1, "Game-windows.zip")], Rules).Problem.Should().Be(ReleaseProblem.NoRelease);
        ReleasePicker.Pick([Rel("v1", false, 1, "Setup.exe")], Rules with { PackageType = "installer" }).Problem.Should().Be(ReleaseProblem.UnsupportedPackage);
    }

    [Fact]
    public void A_release_still_uploading_its_files_is_skipped()
    {
        var pick = ReleasePicker.Pick([Rel("v2", false, 2), Rel("v1", false, 1, "Game-windows.zip")], Rules);
        pick.Choice!.Release.Tag.Should().Be("v1");
    }
}
