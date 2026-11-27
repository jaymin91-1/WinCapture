using System.Buffers.Binary;

namespace WinCapture.Core.Presentation;

/// <summary>One image of an .ico file.</summary>
public sealed record IcoFrame(int Size, int BitCount, int Offset, int Length, bool IsPng);

/// <summary>Minimal ICO directory parser used to pick a frame for a given pixel size (no System.Drawing).</summary>
public static class IcoFile
{
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47 };

    /// <summary>Parses the ICO directory. Returns an empty list when the data is not a valid icon file.</summary>
    public static IReadOnlyList<IcoFrame> ReadFrames(ReadOnlySpan<byte> data)
    {
        var frames = new List<IcoFrame>();
        if (data.Length < 6 || BinaryPrimitives.ReadUInt16LittleEndian(data[..2]) != 0
            || BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(2, 2)) != 1)
        {
            return frames;
        }

        var count = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(4, 2));
        for (var i = 0; i < count; i++)
        {
            var at = 6 + (i * 16);
            if (at + 16 > data.Length)
            {
                return new List<IcoFrame>();
            }

            var size = data[at] == 0 ? 256 : data[at];
            var bits = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(at + 6, 2));
            var length = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(at + 8, 4));
            var offset = (int)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(at + 12, 4));
            if (offset < 0 || length <= 0 || offset > data.Length - length)
            {
                return new List<IcoFrame>();
            }

            var isPng = length >= 4 && data.Slice(offset, 4).SequenceEqual(PngSignature);
            frames.Add(new IcoFrame(size, bits, offset, length, isPng));
        }

        return frames;
    }

    /// <summary>The smallest frame that is at least <paramref name="size"/> px, else the largest one; null when empty.</summary>
    public static IcoFrame? PickFrame(IReadOnlyList<IcoFrame> frames, int size)
    {
        var atLeast = frames.Where(f => f.Size >= size).OrderBy(f => f.Size).ThenByDescending(f => f.BitCount).FirstOrDefault();
        return atLeast ?? frames.OrderByDescending(f => f.Size).ThenByDescending(f => f.BitCount).FirstOrDefault();
    }
}
