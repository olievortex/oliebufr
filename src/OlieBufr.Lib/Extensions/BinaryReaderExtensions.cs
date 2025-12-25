using System.Text;

namespace OlieBufr.Lib.Extensions;

public static class BinaryReaderExtensions
{
    public static byte[] ReadRequired(this BinaryReader br, int length)
    {
        var buffer = br.ReadBytes(length);
        if (buffer.Length < length)
        {
            throw new EndOfStreamException("Could not read enough bytes");
        }
        return buffer;
    }

    public static string ReadFixedLengthString(this BinaryReader br, int length)
    {
        var buffer = br.ReadBytes(length);
        if (buffer.Length < length)
        {
            throw new EndOfStreamException("Could not read enough bytes for fixed length string");
        }
        return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
    }

    public static string ReadLine(this BinaryReader br)
    {
        var sb = new StringBuilder();

        while (true)
        {
            var c = br.ReadByte();
            if (c == '\r') continue;
            if (c == '\n') break;
            sb.Append((char)c);
        }

        return sb.ToString();
    }

    public static int ReadBigEndianInt24(this BinaryReader br)
    {
        var bytes = br.ReadBytes(3);
        if (bytes.Length < 3)
        {
            throw new EndOfStreamException("Could not read enough bytes for Int24");
        }
        return (bytes[0] << 16) | (bytes[1] << 8) | bytes[2];
    }

    public static int ReadBigEndianInt16(this BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        if (bytes.Length < 2)
        {
            throw new EndOfStreamException("Could not read enough bytes for Int16");
        }
        return (bytes[0] << 8) | bytes[1];
    }

    public static int? ReadOptionalInt32(this BinaryReader br)
    {
        var bytes = br.ReadBytes(4);

        if (bytes.Length == 0) return null;

        return BitConverter.ToInt32(bytes, 0);
    }

    public static byte[] ReadAllBytes(this BinaryReader br)
    {
        var result = new List<byte[]>();

        while (true)
        {
            var readings = br.ReadBytes(8192);
            if (readings.Length == 0) break;

            result.Add(readings);
        }

        return [.. result.SelectMany(x => x)];
    }
}
