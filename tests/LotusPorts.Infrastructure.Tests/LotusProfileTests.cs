using LotusPorts.Infrastructure.Lotus;

namespace LotusPorts.Infrastructure.Tests;

public sealed class LotusProfileTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "lotusports-profile-" + Guid.NewGuid().ToString("N"));

    public LotusProfileTests() => Directory.CreateDirectory(folder);

    public void Dispose() => Directory.Delete(folder, recursive: true);

    private LotusProfile? Read(string? json)
    {
        if (json is not null) File.WriteAllText(Path.Combine(folder, "preferences.json"), json);
        return new LotusProfileReader(folder).Read();
    }

    [Fact]
    public void Reads_name_picture_and_accent_from_lotus()
    {
        var picture = Path.Combine(folder, "profile-picture.png");
        File.WriteAllBytes(picture, [0x89, 0x50, 0x4E, 0x47]);
        var json = $$"""{ "Onboarded": true, "DisplayName": "  Joe  ", "AvatarPath": {{System.Text.Json.JsonSerializer.Serialize(picture)}}, "Accent": "crimson" }""";
        Read(json).Should().Be(new LotusProfile("Joe", picture, "crimson"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("""{ "Onboarded": false, "DisplayName": "Joe" }""")]
    [InlineData("""{ "Onboarded": true, "DisplayName": "   " }""")]
    [InlineData("""{ "DisplayName": "Joe" }""")]
    [InlineData("not json")]
    [InlineData("[]")]
    public void No_profile_until_lotus_welcome_is_finished(string? json) => Read(json).Should().BeNull();

    [Theory]
    [InlineData("C:\\does-not-exist\\me.png")]
    [InlineData("relative.png")]
    [InlineData("C:\\Windows\\System32\\notepad.exe")]
    public void Missing_or_non_picture_avatar_is_ignored(string avatar)
    {
        var json = $$"""{ "Onboarded": true, "DisplayName": "Joe", "AvatarPath": {{System.Text.Json.JsonSerializer.Serialize(avatar)}} }""";
        Read(json)!.AvatarPath.Should().BeNull();
    }
}
