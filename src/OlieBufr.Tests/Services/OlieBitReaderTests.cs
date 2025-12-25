using OlieBufr.Lib.Services;

namespace OlieBufr.Tests.Services;

public class OlieBitReaderTests
{
    [Fact]
    public void ReadLeftAlignedBits()
    {
        var data = new byte[] { 0b10101010, 0b11001100 };
        var reader = new OlieBitReader(data);
        var result = reader.ReadBits(4);
        Assert.Equal(0b1010, result);
        result = reader.ReadBits(4);
        Assert.Equal(0b1010, result);
        result = reader.ReadBits(8);
        Assert.Equal(0b11001100, result);
    }

    [Fact]
    public void ReadUnalignedBits()
    {
        var data = new byte[] { 0b10101010, 0b11001100, 0b11000000 };
        var reader = new OlieBitReader(data)
        {
            BitPosition = 3
        };
        var result = reader.ReadBits(5);
        Assert.Equal(0b01010, result);
        result = reader.ReadBits(7);
        Assert.Equal(0b1100110, result);
        result = reader.ReadBits(4);
        Assert.Equal(0b0110, result);
    }

    [Fact]
    public void ReadAlignedByte()
    {
        var data = new byte[] { 0b10101010, 0b11001100 };
        var reader = new OlieBitReader(data);

        var result = reader.ReadByte();
        Assert.Equal(0b10101010, result);
    }

    [Fact]
    public void ReadUnalignedByte()
    {
        var data = new byte[] { 0b10101010, 0b11001100 };
        var reader = new OlieBitReader(data)
        {
            BitPosition = 3
        };

        var result = reader.ReadByte();
        Assert.Equal(0b01010110, result);
    }

    [Fact]
    public void ReadFixedLengthString_Aligned()
    {
        var data = "HELLO"u8.ToArray(); // "HELLO"
        var reader = new OlieBitReader(data);
        var result = reader.ReadFixedLengthString(5);
        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void ReadFixedLengthString_TrimsTrailingNullsAndSpaces()
    {
        var data = new byte[] { 0x54, 0x45, 0x53, 0x54, 0x00, 0x20 }; // "TEST", 0x00, ' '
        var reader = new OlieBitReader(data);
        var result = reader.ReadFixedLengthString(6);
        Assert.Equal("TEST", result);
    }

    [Fact]
    public void ReadFixedLengthString_UnalignedReads()
    {
        var data = new byte[] { 0b10101010, 0b11001100 }; // matches existing unaligned byte test
        var reader = new OlieBitReader(data) { BitPosition = 3 };
        var result = reader.ReadFixedLengthString(1);
        Assert.Equal("V", result); // 0b01010110 == 0x56 == 'V'
    }

    [Fact]
    public void ReadFixedLengthString_ZeroLengthReturnsEmpty()
    {
        var data = new byte[] { 0x00, 0x01 };
        var reader = new OlieBitReader(data);
        var result = reader.ReadFixedLengthString(0);
        Assert.Equal(string.Empty, result);
    }
}
