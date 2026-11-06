using System.Security.Cryptography;
using System.Text;
using LotusPorts.Core.GameFiles;

namespace LotusPorts.Core.Tests;

public sealed class GameFileReaderTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));

    public GameFileReaderTests() => Directory.CreateDirectory(folder);

    public void Dispose()
    {
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    /// <summary>A fake canonical (.z64) cartridge: real header layout, made-up content.</summary>
    private static byte[] FakeZ64(int size = 3 * (1 << 20) + 64)
    {
        var rom = new byte[size];
        for (var i = 0; i < rom.Length; i++) rom[i] = (byte)(i * 31 + 7);
        new byte[] { 0x80, 0x37, 0x12, 0x40 }.CopyTo(rom, 0);
        Encoding.ASCII.GetBytes("SAMPLE GAME         ").CopyTo(rom, 0x20);
        Encoding.ASCII.GetBytes("NSXE").CopyTo(rom, 0x3B);
        rom[0x3F] = 1;
        return rom;
    }

    private static byte[] Swap16(byte[] z64)
    {
        var copy = (byte[])z64.Clone();
        for (var i = 0; i + 1 < copy.Length; i += 2) (copy[i], copy[i + 1]) = (copy[i + 1], copy[i]);
        return copy;
    }

    private static byte[] Reverse32(byte[] z64)
    {
        var copy = (byte[])z64.Clone();
        for (var i = 0; i + 3 < copy.Length; i += 4) Array.Reverse(copy, i, 4);
        return copy;
    }

    private string Write(string name, byte[] bytes)
    {
        var path = Path.Combine(folder, name);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    [Fact]
    public async Task The_three_n64_byte_orders_are_identified_and_give_the_same_hashes()
    {
        var z64 = FakeZ64();
        var a = await GameFileReader.ReadAsync(Write("game.z64", z64));
        var b = await GameFileReader.ReadAsync(Write("game.v64", Swap16(z64)));
        var c = await GameFileReader.ReadAsync(Write("game.n64", Reverse32(z64)));

        a.ByteOrder.Should().Be(N64ByteOrder.BigEndian);
        b.ByteOrder.Should().Be(N64ByteOrder.ByteSwapped);
        c.ByteOrder.Should().Be(N64ByteOrder.LittleEndian);
        new[] { a, b, c }.Should().OnlyContain(f => f.Format == GameFileFormat.N64);
        b.Hashes.Should().Be(a.Hashes);
        c.Hashes.Should().Be(a.Hashes);
        a.Hashes!.Sha1.Should().Be(Convert.ToHexStringLower(SHA1.HashData(z64)));
        a.Hashes.Md5.Should().Be(Convert.ToHexStringLower(MD5.HashData(z64)));
    }

    [Fact]
    public async Task The_header_is_read_whatever_the_order()
    {
        var info = await GameFileReader.ReadAsync(Write("game.v64", Swap16(FakeZ64())));
        info.Header.Should().Be(new N64Header("SAMPLE GAME", "NSXE", 1));
        info.Header!.RegionCode.Should().Be('E');
    }

    [Fact]
    public async Task The_original_file_is_never_changed()
    {
        var path = Write("game.n64", Reverse32(FakeZ64()));
        var before = SHA256.HashData(File.ReadAllBytes(path));
        var modified = File.GetLastWriteTimeUtc(path);

        await GameFileReader.ReadAsync(path);
        await GameFileReader.WriteCanonicalCopyAsync(path, Path.Combine(folder, "copy.z64"));

        SHA256.HashData(File.ReadAllBytes(path)).Should().Equal(before);
        File.GetLastWriteTimeUtc(path).Should().Be(modified);
    }

    [Fact]
    public async Task The_converted_copy_is_canonical()
    {
        var z64 = FakeZ64();
        var copy = Path.Combine(folder, "copy.z64");
        await GameFileReader.WriteCanonicalCopyAsync(Write("game.v64", Swap16(z64)), copy);
        File.ReadAllBytes(copy).Should().Equal(z64);
    }

    [Fact]
    public async Task Crc32_matches_the_standard_check_value()
    {
        using var stream = new MemoryStream("123456789"u8.ToArray());
        (await GameFileReader.HashAsync(stream, null)).Crc32.Should().Be("cbf43926");
    }

    [Fact]
    public async Task A_file_named_z64_without_n64_content_is_unknown_and_large_files_are_not_hashed()
    {
        (await GameFileReader.ReadAsync(Write("fake.z64", "<html>not a game</html>"u8.ToArray()))).Format.Should().Be(GameFileFormat.Unknown);
        var big = await GameFileReader.ReadAsync(Write("disc.iso", new byte[4096]), maxHashBytes: 1024);
        big.Format.Should().Be(GameFileFormat.DiscImage);
        big.Hashes.Should().BeNull();
    }
}
