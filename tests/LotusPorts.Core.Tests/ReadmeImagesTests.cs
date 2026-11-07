using LotusPorts.Core.Guides;

namespace LotusPorts.Core.Tests;

public sealed class ReadmeImagesTests
{
    private static readonly Uri Base = new("https://raw.githubusercontent.com/owner/port/HEAD/");

    [Fact]
    public void Screenshots_are_kept_and_badges_logos_and_other_hosts_are_left_out()
    {
        const string md = """
            [![Discord](https://img.shields.io/discord/123?label=chat)](https://discord.gg/x)
            <img src="docs/logo.png" width="200">
            ![Title screen](docs/screens/title.png)
            ![Gameplay](https://github.com/owner/port/blob/main/docs/play.jpg "In game")
            <img alt="x" src="https://github.com/user-attachments/assets/1234-abcd">
            ![tracker](https://evil.example.com/pixel.gif)
            ![vector](docs/diagram.svg)
            ![Title screen again](docs/screens/title.png)
            """;
        ReadmeImages.Extract(md, Base).Select(u => u.AbsoluteUri).Should().Equal(
            "https://raw.githubusercontent.com/owner/port/HEAD/docs/screens/title.png",
            "https://github.com/owner/port/raw/main/docs/play.jpg",
            "https://github.com/user-attachments/assets/1234-abcd");
    }

    [Fact]
    public void At_most_six_pictures()
    {
        var md = string.Join("\n", Enumerable.Range(1, 10).Select(i => $"![s{i}](shots/{i}.png)"));
        ReadmeImages.Extract(md, Base).Should().HaveCount(ReadmeImages.Max);
    }
}
