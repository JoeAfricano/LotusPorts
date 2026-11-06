using AwesomeAssertions;
using LotusPorts.Infrastructure.Network;
using LotusPorts.Infrastructure.Updates;
using Xunit;

namespace LotusPorts.Infrastructure.Tests;

public sealed class AppUpdatePickerTests
{
    private static readonly string Sha = new('a', 64);

    private static ReleaseInfo Release(string tag, bool pre = false, string asset = "LotusPorts-Setup.exe", string? sha = null) =>
        new(tag, tag, pre, DateTimeOffset.UnixEpoch, [new ReleaseAssetInfo(asset, 100, sha ?? Sha, new Uri("https://github.com/x/y/releases/download/" + tag + "/" + asset))]);

    [Fact]
    public void Newer_final_release_with_a_hashed_installer_is_offered()
    {
        var pick = AppUpdatePicker.Pick([Release("v0.1.0"), Release("v0.2.0"), Release("v0.1.5")], "0.1.0");
        pick!.Tag.Should().Be("v0.2.0");
    }

    [Fact]
    public void Same_or_older_versions_are_not_offered() =>
        AppUpdatePicker.Pick([Release("v0.1.0"), Release("0.0.9")], "0.1.0").Should().BeNull();

    [Fact]
    public void Test_versions_reach_only_people_on_a_test_version()
    {
        AppUpdatePicker.Pick([Release("v0.3.0-alpha", pre: true)], "0.2.0").Should().BeNull();
        AppUpdatePicker.Pick([Release("v0.3.0-alpha", pre: true)], "0.2.0-alpha")!.Tag.Should().Be("v0.3.0-alpha");
    }

    [Fact]
    public void The_final_build_replaces_its_own_test_build() =>
        AppUpdatePicker.Pick([Release("v0.2.0")], "0.2.0-alpha")!.Tag.Should().Be("v0.2.0");

    [Fact]
    public void Releases_without_an_installer_or_its_sha256_are_skipped()
    {
        AppUpdatePicker.Pick([Release("v0.2.0", asset: "LotusPorts.zip")], "0.1.0").Should().BeNull();
        AppUpdatePicker.Pick([Release("v0.2.0", sha: "")], "0.1.0").Should().BeNull();
    }

    [Theory]
    [InlineData("v1.2.3", 1, 2, 3, false)]
    [InlineData("1.2.3-alpha", 1, 2, 3, true)]
    [InlineData("v0.2-beta.1", 0, 2, 0, true)]
    public void Tags_are_read(string tag, int major, int minor, int build, bool pre)
    {
        var v = AppUpdatePicker.Parse(tag)!.Value;
        v.Version.Should().Be(new Version(major, minor, build));
        v.Prerelease.Should().Be(pre);
    }

    [Fact]
    public void Text_that_is_not_a_version_is_ignored() => AppUpdatePicker.Parse("nightly").Should().BeNull();
}
