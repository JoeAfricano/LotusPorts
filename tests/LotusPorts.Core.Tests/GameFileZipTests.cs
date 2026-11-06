using System.IO.Compression;
using LotusPorts.Core.GameFiles;

namespace LotusPorts.Core.Tests;

/// <summary>Game files kept inside a .zip are copied out (the .zip only read) and then read like any other file.</summary>
public sealed class GameFileZipTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "LotusPortsTests", Guid.NewGuid().ToString("N"));

    public GameFileZipTests() => Directory.CreateDirectory(folder);

    public void Dispose()
    {
        try { Directory.Delete(folder, true); } catch (IOException) { }
    }

    private string Zip(params (string Name, byte[] Data)[] entries)
    {
        var file = Path.Combine(folder, "download.zip");
        using var archive = ZipFile.Open(file, ZipArchiveMode.Create);
        foreach (var (name, data) in entries)
            using (var s = archive.CreateEntry(name).Open()) s.Write(data);
        return file;
    }

    private static byte[] N64Rom()
    {
        var rom = new byte[4096];
        rom[0] = 0x80; rom[1] = 0x37; rom[2] = 0x12; rom[3] = 0x40; // .z64 (big-endian) signature
        "ZELDA MAJORA'S MASK"u8.CopyTo(rom.AsSpan(0x20));
        return rom;
    }

    [Fact]
    public async Task The_game_file_inside_is_copied_out_and_recognized_as_N64()
    {
        var zip = Zip(("readme.txt", "hi"u8.ToArray()), ("Some Folder/../Game (USA).z64", N64Rom()));
        var before = File.ReadAllBytes(zip);
        var outFolder = Path.Combine(folder, "game-files");

        var copies = await GameFileZip.ExtractCandidatesAsync(zip, outFolder, 1 << 20);

        copies.Should().ContainSingle().Which.Should().Be(Path.Combine(outFolder, "Game (USA).z64"));
        var info = await GameFileReader.ReadAsync(copies[0]);
        info.Format.Should().Be(GameFileFormat.N64);
        info.Hashes.Should().NotBeNull();
        File.ReadAllBytes(zip).Should().Equal(before, "the .zip is only read");

        // Reading it again reuses the copy.
        (await GameFileZip.ExtractCandidatesAsync(zip, outFolder, 1 << 20)).Should().Equal(copies);
    }

    [Fact]
    public async Task A_zip_without_game_files_or_with_huge_ones_copies_nothing()
    {
        var zip = Zip(("notes.txt", "x"u8.ToArray()), ("big.iso", new byte[2048]));
        (await GameFileZip.ExtractCandidatesAsync(zip, Path.Combine(folder, "out"), maxBytes: 1024)).Should().BeEmpty();
        Directory.Exists(Path.Combine(folder, "out")).Should().BeFalse();
    }
}
