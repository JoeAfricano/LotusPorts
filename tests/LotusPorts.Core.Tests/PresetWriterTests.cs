using System.Text.Json;
using LotusPorts.Core.Styles;

namespace LotusPorts.Core.Tests;

public class PresetWriterTests
{
    private static IReadOnlyDictionary<string, JsonElement> Values(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

    [Fact]
    public void Json_style_is_applied_and_reverted_without_losing_other_settings()
    {
        const string config = """{ "graphics": { "resolution": "1x", "vsync": true }, "audio": { "volume": 70 }, "player": "Joe" }""";
        var remaster = Values("""{ "graphics.resolution": "4x", "graphics.widescreen": true, "graphics.fps": 60 }""");

        var (applied, previous) = PresetWriter.Apply(config, "json", remaster);
        PresetWriter.Verify(applied, "json", remaster).Should().BeTrue();
        var keep = PresetWriter.Read(applied, "json", ["graphics.vsync", "audio.volume", "player"]);
        keep.Should().Equal(new Dictionary<string, string?> { ["graphics.vsync"] = "true", ["audio.volume"] = "70", ["player"] = "\"Joe\"" });

        // The player changes something else meanwhile; reverting the style keeps it.
        var changedByPlayer = PresetWriter.Apply(applied, "json", Values("""{ "audio.volume": 40 }""")).Text;
        var reverted = PresetWriter.Revert(changedByPlayer, "json", previous);
        var after = PresetWriter.Read(reverted, "json", ["graphics.resolution", "graphics.widescreen", "graphics.fps", "graphics.vsync", "audio.volume", "player"]);
        after["graphics.resolution"].Should().Be("\"1x\"");
        after["graphics.widescreen"].Should().BeNull("the key did not exist before the style");
        after["graphics.fps"].Should().BeNull();
        after["graphics.vsync"].Should().Be("true");
        after["audio.volume"].Should().Be("40");
        after["player"].Should().Be("\"Joe\"");
    }

    [Fact]
    public void Ini_style_keeps_comments_order_and_other_keys()
    {
        const string config = "; game settings\r\nlanguage=pt\r\n[Video]\r\nWidth=640\r\nVSync=1\r\n[Audio]\r\nVolume=80\r\n";
        var remaster = Values("""{ "Video.Width": 1920, "Video.Widescreen": true, "Enhancements.Fps": "60" }""");

        var (applied, previous) = PresetWriter.Apply(config, "ini", remaster);

        PresetWriter.Verify(applied, "ini", remaster).Should().BeTrue();
        applied.Should().StartWith("; game settings\r\nlanguage=pt\r\n[Video]\r\nWidth=1920\r\nVSync=1\r\nWidescreen=true\r\n[Audio]\r\nVolume=80\r\n");
        applied.Should().Contain("[Enhancements]\r\nFps=60");

        var reverted = PresetWriter.Revert(applied, "ini", previous);
        reverted.Should().Be(config + "[Enhancements]\r\n");
        PresetWriter.Read(reverted, "ini", ["Video.Width", "Audio.Volume", "language"]).Values.Should().Equal("640", "80", "pt");
    }

    [Fact]
    public void Unknown_formats_are_not_written()
    {
        PresetWriter.Supports("toml").Should().BeFalse();
        var act = () => PresetWriter.Apply("a = 1", "toml", Values("""{ "a": 2 }"""));
        act.Should().Throw<NotSupportedException>();
    }
}
