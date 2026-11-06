using LotusPorts.Core.Install;

namespace LotusPorts.Core.Tests;

public class PlayButtonTests
{
    private static PlayContext Ctx(bool building = false, bool canInstall = true, bool installed = false, bool working = false, bool running = false,
        bool update = false, bool auto = false, bool problem = false, bool rollback = false, bool fileMissing = false) =>
        new(building, canInstall, installed, working, running, update, auto, problem, rollback, fileMissing);

    [Theory]
    [InlineData(true, true, PlayButtonState.Follow)]
    [InlineData(false, true, PlayButtonState.Install)]
    [InlineData(false, false, PlayButtonState.OfficialPage)]
    public void Not_installed(bool building, bool canInstall, PlayButtonState expected) =>
        PlayButtonRules.Decide(Ctx(building, canInstall)).State.Should().Be(expected);

    [Fact]
    public void Working_and_running_win_over_everything()
    {
        PlayButtonRules.Decide(Ctx(installed: true, working: true, running: true, problem: true)).Should()
            .Match<PlayButton>(b => b.State == PlayButtonState.Working && b.Secondary.SequenceEqual(new[] { SecondaryAction.Cancel }));
        PlayButtonRules.Decide(Ctx(installed: true, running: true, problem: true)).State.Should().Be(PlayButtonState.Running);
    }

    [Fact]
    public void Updates_follow_the_automatic_update_setting()
    {
        var auto = PlayButtonRules.Decide(Ctx(installed: true, update: true, auto: true));
        auto.State.Should().Be(PlayButtonState.UpdateAndPlay);
        auto.Secondary.Should().StartWith(SecondaryAction.PlayWithoutUpdate);

        var manual = PlayButtonRules.Decide(Ctx(installed: true, update: true, auto: false));
        manual.State.Should().Be(PlayButtonState.Play);
        manual.Secondary.Should().StartWith(SecondaryAction.Update);
    }

    [Fact]
    public void A_failed_first_launch_offers_rollback_first()
    {
        var button = PlayButtonRules.Decide(Ctx(installed: true, problem: true, rollback: true, update: true));
        button.State.Should().Be(PlayButtonState.Resolve);
        button.Secondary.Should().StartWith(SecondaryAction.Rollback);
        PlayButtonRules.Decide(Ctx(installed: true, problem: true)).Secondary.Should().NotContain(SecondaryAction.Rollback);
    }

    [Fact]
    public void Ready_installed_game_plays_and_can_be_managed()
    {
        var button = PlayButtonRules.Decide(Ctx(installed: true));
        button.State.Should().Be(PlayButtonState.Play);
        button.Secondary.Should().Contain([SecondaryAction.OpenFolder, SecondaryAction.Uninstall]).And.NotContain(SecondaryAction.Rollback);
        PlayButtonRules.Decide(Ctx(installed: true, fileMissing: true)).State.Should().Be(PlayButtonState.AddGameFile);
    }

    [Theory]
    [InlineData("v1.2.2", "v1.2.2")]
    [InlineData("release/2024:05", "release_2024_05")]
    [InlineData("..", "version")]
    [InlineData("CON", "_CON")]
    [InlineData(" 1.0. ", "1.0")]
    public void Version_folders_are_safe_names(string tag, string folder) => VersionFolders.For(tag).Should().Be(folder);
}
