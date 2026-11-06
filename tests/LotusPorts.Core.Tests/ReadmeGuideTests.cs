using LotusPorts.Core.Guides;

namespace LotusPorts.Core.Tests;

public class ReadmeGuideTests
{
    private static readonly Uri Base = new("https://github.com/owner/repo/blob/HEAD/");

    private const string QuickStartReadme = """
        # Sample Port
        ![logo](docs/logo.png)
        <!-- badges -->
        Some intro.

        # Quick Start
        ### 1. Check your copy of the game
        Use the [checker](https://example.com/check) to confirm your **copy** works.
        ### 2. Download the game
        Get it from [Releases](../../releases).
        ### 3. Launch
        #### Windows
        - Extract the zip
        - Run `game.exe` and pick your file
        > [!NOTE]
        > Settings are saved next to the program.
        #### Linux
        - chmod +x game.appimage
        #### macOS
        - Open the dmg

        # Development
        ### Building
        Run cmake.
        """;

    [Fact]
    public void Quick_start_keeps_windows_steps_and_drops_other_systems_and_building()
    {
        var guide = ReadmeGuide.Extract(QuickStartReadme, Base)!;

        guide.SectionTitles.Should().Equal("Quick Start");
        var text = guide.Blocks.Select(b => $"{b.Kind}:{b.PlainText}").ToList();
        text.Should().ContainInOrder(
            "Heading:1. Check your copy of the game",
            "Paragraph:Use the checker to confirm your copy works.",
            "Heading:2. Download the game",
            "Paragraph:Get it from Releases.",
            "Heading:3. Launch",
            "Heading:Windows",
            "Bullet:Extract the zip",
            "Bullet:Run game.exe and pick your file");
        text.Should().NotContain(t => t.Contains("chmod") || t.Contains("dmg") || t.Contains("cmake") || t.Contains("Linux") || t.Contains("macOS") || t.Contains("logo"));
    }

    [Fact]
    public void Links_are_resolved_against_the_repository_and_code_and_bold_are_marked()
    {
        var blocks = ReadmeGuide.Extract(QuickStartReadme, Base)!.Blocks;
        var spans = blocks.SelectMany(b => b.Spans).ToList();
        spans.Should().Contain(s => s.Text == "checker" && s.Link == new Uri("https://example.com/check"));
        spans.Should().Contain(s => s.Text == "Releases" && s.Link == new Uri("https://github.com/owner/repo/releases"));
        spans.Should().Contain(s => s.Text == "game.exe" && s.Code);
        spans.Should().Contain(s => s.Text == "copy" && s.Bold);
        blocks.Should().Contain(b => b.Kind == GuideBlockKind.Note && b.PlainText == "Settings are saved next to the program.");
    }

    [Fact]
    public void A_numbered_step_heading_stays_even_without_text_under_it()
    {
        var guide = ReadmeGuide.Extract("# Quick Start\n### 1. Check\nOk.\n### 2. Download from [Releases](https://github.com/o/r/releases)\n### 3. Play\nHave fun.", Base)!;
        guide.Blocks.Select(b => b.PlainText).Should().Equal("1. Check", "Ok.", "2. Download from Releases", "3. Play", "Have fun.");
    }

    [Fact]
    public void No_usage_section_means_no_guide()
    {
        ReadmeGuide.Extract("# Game\n## Features\nFast.\n## Building\nRun make.\n## FAQ\n### How do I set up gyro on Steam Deck?\nLike this.", Base).Should().BeNull();
    }

    [Fact]
    public void How_to_install_with_numbered_steps_and_code()
    {
        var md = """
            ## How to Install
            1. Download the installer.
            2. Point it to your **game folder**.
            ```
            game.exe --setup
            ```
            ## Features
            Lots.
            """;
        var blocks = ReadmeGuide.Extract(md, Base)!.Blocks;
        blocks.Select(b => (b.Kind, b.Number, b.PlainText)).Should().Equal(
            (GuideBlockKind.Numbered, 1, "Download the installer."),
            (GuideBlockKind.Numbered, 2, "Point it to your game folder."),
            (GuideBlockKind.Code, 0, "game.exe --setup"));
    }

    [Theory]
    [InlineData("https://files.example/zelda.z64")]
    [InlineData("https://cdn.example/game.iso")]
    [InlineData("https://cdn.example/bios.bin")]
    public void Links_to_game_files_lose_the_link_but_keep_the_text(string url)
    {
        var guide = ReadmeGuide.Extract($"## Usage\nGet [this file]({url}) first.", Base)!;
        var span = guide.Blocks.Single().Spans.Single(s => s.Text == "this file");
        span.Link.Should().BeNull();
    }

    [Fact]
    public void At_most_two_sections_and_nested_sections_are_not_repeated()
    {
        var md = "## Installation\nA.\n### Setup\nB.\n## Usage\nC.\n## Running the game\nD.\n";
        var guide = ReadmeGuide.Extract(md, Base)!;
        guide.SectionTitles.Should().Equal("Installation", "Usage");
        guide.Blocks.Select(b => b.PlainText).Should().Equal("A.", "Setup", "B.", "Usage", "C.");
    }
}
