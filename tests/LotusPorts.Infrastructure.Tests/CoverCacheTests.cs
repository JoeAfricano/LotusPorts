using LotusPorts.Infrastructure.Catalog;

namespace LotusPorts.Infrastructure.Tests;

public class CoverCacheTests
{
    [Fact]
    public void Png_and_jpeg_are_covers()
    {
        CoverCache.IsImage([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]).Should().BeTrue();
        CoverCache.IsImage([0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49]).Should().BeTrue();
    }

    [Fact]
    public void Anything_else_is_not_a_cover()
    {
        CoverCache.IsImage(null).Should().BeFalse();
        CoverCache.IsImage([0xFF, 0xD8]).Should().BeFalse();
        CoverCache.IsImage("<!doctype html>"u8.ToArray()).Should().BeFalse();
        CoverCache.IsImage([0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00, 0x08]).Should().BeFalse();
    }
}
