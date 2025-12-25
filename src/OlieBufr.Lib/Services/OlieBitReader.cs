using System.Text;

namespace OlieBufr.Lib.Services;

public class OlieBitReader(byte[] data)
{
    public byte[] Data { get; } = data;
    public int BitPosition { get; set; } = 0;

    public int ReadBits(int bits)
    {
        var result = 0;

        while (bits > 0)
        {
            var byteIndex = BitPosition / 8;
            var bitsConsumed = BitPosition % 8;

            var value = Data[byteIndex];
            var mask = (byte)(0xFF >> bitsConsumed);
            value &= mask;

            var bitsRemaining = 8 - bitsConsumed;
            var bitsToRead = Math.Min(bits, bitsRemaining);
            var bitsToShift = bitsRemaining - bitsToRead;

            value >>>= bitsToShift;

            result <<= bitsToRead;
            result |= value;

            bits -= bitsToRead;
            BitPosition += bitsToRead;
        }

        return result;
    }

    public byte ReadByte()
    {
        var byteIndex = BitPosition / 8;

        var value = Data[byteIndex];
        value <<= (BitPosition % 8);
        var rs = 8 - (BitPosition % 8);
        if (rs != 8)
        {
            value |= (byte)(Data[byteIndex + 1] >>> rs);
        }
        BitPosition += 8;

        return value;
    }

    public string ReadFixedLengthString(int length)
    {
        var bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = ReadByte();
        }
        return Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ').TrimEnd();
    }
}
