using System.Buffers.Binary;
using System.Text;

namespace LotusPorts.Core.GameFiles;

/// <summary>The three ways N64 cartridge dumps are stored (master prompt, section 8.3).</summary>
public enum N64ByteOrder
{
    /// <summary>.z64, big-endian: the canonical order. Starts with 80 37 12 40.</summary>
    BigEndian,

    /// <summary>.v64, every 16-bit pair swapped. Starts with 37 80 40 12.</summary>
    ByteSwapped,

    /// <summary>.n64, every 32-bit word reversed. Starts with 40 12 37 80.</summary>
    LittleEndian,
}

/// <summary>What the cartridge header says (read after converting to the canonical order).</summary>
public sealed record N64Header(string InternalName, string GameCode, byte Version)
{
    /// <summary>Region letter of the game code (4th character): E = USA, P = Europe, J = Japan…</summary>
    public char RegionCode => GameCode.Length == 4 ? GameCode[3] : '?';
}

public static class N64Rom
{
    public const int HeaderSize = 0x40;

    public static N64ByteOrder? Detect(ReadOnlySpan<byte> start)
    {
        if (start.Length < 4) return null;
        return (start[0], start[1], start[2], start[3]) switch
        {
            (0x80, 0x37, 0x12, 0x40) => N64ByteOrder.BigEndian,
            (0x37, 0x80, 0x40, 0x12) => N64ByteOrder.ByteSwapped,
            (0x40, 0x12, 0x37, 0x80) => N64ByteOrder.LittleEndian,
            _ => null,
        };
    }

    /// <summary>Converts a chunk to the canonical (.z64) order in place. Bytes past the last whole word stay as they are.</summary>
    public static void ToCanonical(Span<byte> chunk, N64ByteOrder order)
    {
        switch (order)
        {
            case N64ByteOrder.ByteSwapped:
                for (var i = 0; i + 1 < chunk.Length; i += 2) (chunk[i], chunk[i + 1]) = (chunk[i + 1], chunk[i]);
                break;
            case N64ByteOrder.LittleEndian:
                for (var i = 0; i + 3 < chunk.Length; i += 4)
                    BinaryPrimitives.WriteUInt32BigEndian(chunk[i..], BinaryPrimitives.ReadUInt32LittleEndian(chunk[i..]));
                break;
        }
    }

    /// <summary>Reads the header of a canonical (.z64) dump: internal name at 0x20, game code at 0x3B, version at 0x3F.</summary>
    public static N64Header? ReadHeader(ReadOnlySpan<byte> canonical)
    {
        if (canonical.Length < HeaderSize || Detect(canonical) != N64ByteOrder.BigEndian) return null;
        var name = Latin1(canonical.Slice(0x20, 20)).Trim('\0', ' ');
        var code = Latin1(canonical.Slice(0x3B, 4));
        if (code.Any(c => !char.IsAsciiLetterOrDigit(c))) code = "";
        return new N64Header(name, code, canonical[0x3F]);
    }

    private static string Latin1(ReadOnlySpan<byte> bytes)
    {
        var text = Encoding.Latin1.GetString(bytes);
        return new string(text.Select(c => c is >= ' ' and <= '~' ? c : ' ').ToArray());
    }
}
