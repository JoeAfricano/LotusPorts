using System.Security.Cryptography;

namespace LotusPorts.Core.GameFiles;

public enum GameFileFormat
{
    Unknown,
    N64,
    Snes,
    Gba,
    Nds,
    DiscImage,
    Xbox360Executable,
    WiiUDump,
}

/// <summary>Hashes of the file's content (for N64, of the canonical .z64 order), lowercase hex.</summary>
public sealed record GameFileHashes(string Sha1, string Md5, string Sha256, string Crc32);

/// <summary>What was read from one file. <see cref="Hashes"/> is null when the file is too large to hash.</summary>
public sealed record GameFileInfo(
    string Path,
    long Size,
    DateTime ModifiedUtc,
    GameFileFormat Format,
    N64ByteOrder? ByteOrder,
    N64Header? Header,
    GameFileHashes? Hashes);

/// <summary>
/// Reads game files the player owns, only to recognize them: the file is opened read-only (shared, never locked for
/// writing) and nothing is ever written, moved or renamed. Hashes are computed in one streaming pass; for N64 they are
/// computed over the canonical (.z64) order, so .z64, .v64 and .n64 copies of the same game give the same hashes.
/// </summary>
public static class GameFileReader
{
    /// <summary>Files larger than this are listed by format only (hashing a whole disc image would take long).</summary>
    public const long DefaultMaxHashBytes = 1L * 1024 * 1024 * 1024;

    private static readonly Dictionary<string, GameFileFormat> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".z64"] = GameFileFormat.N64, [".v64"] = GameFileFormat.N64, [".n64"] = GameFileFormat.N64,
        [".sfc"] = GameFileFormat.Snes, [".smc"] = GameFileFormat.Snes,
        [".gba"] = GameFileFormat.Gba, [".nds"] = GameFileFormat.Nds,
        [".iso"] = GameFileFormat.DiscImage, [".bin"] = GameFileFormat.DiscImage, [".cue"] = GameFileFormat.DiscImage,
        [".chd"] = GameFileFormat.DiscImage, [".cso"] = GameFileFormat.DiscImage, [".gcm"] = GameFileFormat.DiscImage,
        [".rvz"] = GameFileFormat.DiscImage, [".img"] = GameFileFormat.DiscImage,
        [".xex"] = GameFileFormat.Xbox360Executable,
        [".wux"] = GameFileFormat.WiiUDump, [".wud"] = GameFileFormat.WiiUDump, [".wua"] = GameFileFormat.WiiUDump,
    };

    /// <summary>Extensions the scanner looks at. ".rom" is included because some N64 dumps use it (checked by content).</summary>
    public static bool IsCandidate(string path) =>
        Extensions.ContainsKey(System.IO.Path.GetExtension(path)) || string.Equals(System.IO.Path.GetExtension(path), ".rom", StringComparison.OrdinalIgnoreCase);

    public static async Task<GameFileInfo> ReadAsync(string path, long maxHashBytes = DefaultMaxHashBytes, CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(path);
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16, useAsync: true);
        var head = new byte[N64Rom.HeaderSize];
        var headLength = await stream.ReadAtLeastAsync(head, head.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
        var order = N64Rom.Detect(head.AsSpan(0, headLength));
        var format = order is not null ? GameFileFormat.N64 : Extensions.GetValueOrDefault(info.Extension, GameFileFormat.Unknown);
        if (format == GameFileFormat.N64 && order is null) format = GameFileFormat.Unknown; // .z64 name without N64 content

        N64Header? header = null;
        if (order is { } o)
        {
            var canonical = head.AsSpan(0, headLength).ToArray();
            N64Rom.ToCanonical(canonical, o);
            header = N64Rom.ReadHeader(canonical);
        }

        GameFileHashes? hashes = null;
        if (format != GameFileFormat.Unknown && info.Length <= maxHashBytes)
        {
            stream.Position = 0;
            hashes = await HashAsync(stream, order, cancellationToken).ConfigureAwait(false);
        }
        return new GameFileInfo(info.FullName, info.Length, info.LastWriteTimeUtc, format, order, header, hashes);
    }

    /// <summary>SHA-1, MD5, SHA-256 and CRC32 in one pass; N64 content is converted to .z64 order in memory first.</summary>
    public static async Task<GameFileHashes> HashAsync(Stream stream, N64ByteOrder? order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        using var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var crc = Crc32.Start;
        var buffer = new byte[1 << 20]; // a multiple of 4, so N64 words never straddle two reads
        int read;
        while ((read = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false)) > 0)
        {
            var chunk = buffer.AsSpan(0, read);
            if (order is { } o && o != N64ByteOrder.BigEndian) N64Rom.ToCanonical(chunk, o);
            sha1.AppendData(chunk);
            md5.AppendData(chunk);
            sha256.AppendData(chunk);
            crc = Crc32.Append(crc, chunk);
            if (read < buffer.Length) break;
        }
        return new GameFileHashes(
            Convert.ToHexStringLower(sha1.GetHashAndReset()),
            Convert.ToHexStringLower(md5.GetHashAndReset()),
            Convert.ToHexStringLower(sha256.GetHashAndReset()),
            Crc32.Finish(crc).ToString("x8", System.Globalization.CultureInfo.InvariantCulture));
    }

    /// <summary>Writes a canonical (.z64) copy of an N64 dump to <paramref name="destination"/>. The original is only read.</summary>
    public static async Task WriteCanonicalCopyAsync(string source, string destination, CancellationToken cancellationToken = default)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16, useAsync: true);
        var head = new byte[4];
        var n = await input.ReadAtLeastAsync(head, 4, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false);
        var order = N64Rom.Detect(head.AsSpan(0, n)) ?? throw new InvalidDataException("Not an N64 dump.");
        input.Position = 0;
        var temp = destination + ".part";
        await using (var output = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
        {
            var buffer = new byte[1 << 20];
            int read;
            while ((read = await input.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false)) > 0)
            {
                N64Rom.ToCanonical(buffer.AsSpan(0, read), order);
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                if (read < buffer.Length) break;
            }
        }
        File.Move(temp, destination);
    }
}

/// <summary>CRC-32 (IEEE 802.3), as used by No-Intro and most port READMEs.</summary>
public static class Crc32
{
    private static readonly uint[] Table = Build();

    public const uint Start = 0xFFFFFFFF;

    public static uint Append(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var b in data) crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc;
    }

    public static uint Finish(uint crc) => ~crc;

    private static uint[] Build()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var c = i;
            for (var k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[i] = c;
        }
        return table;
    }
}
