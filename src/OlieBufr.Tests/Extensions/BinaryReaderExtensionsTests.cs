using OlieBufr.Lib.Extensions;

namespace OlieBufr.Tests.Extensions;

public class BinaryReaderExtensionsTests
{
    #region ReadRequired

    [Fact]
    public void ReadRequired_ReturnsBytes_WhenEnoughData()
    {
        var data = new byte[] { 1, 2, 3, 4, 5 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadRequired(3);

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void ReadRequired_ThrowsEndOfStreamException_WhenNotEnoughData()
    {
        var data = new byte[] { 1, 2 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadRequired(3));
    }

    [Fact]
    public void ReadRequired_ReturnsEmptyArray_WhenLengthIsZero()
    {
        using var ms = new MemoryStream([]);
        using var br = new BinaryReader(ms);

        var result = br.ReadRequired(0);

        Assert.Empty(result);
    }

    #endregion

    #region ReadFixedLengthString

    [Fact]
    public void ReadFixedLengthString_ReturnsString_WhenExactLength()
    {
        var data = "ABC"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadFixedLengthString(3);

        Assert.Equal("ABC", result);
    }

    [Fact]
    public void ReadFixedLengthString_TrimsTrailingNulls()
    {
        var data = new byte[] { (byte)'A', (byte)'B', (byte)'C', 0, 0 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadFixedLengthString(5);

        Assert.Equal("ABC", result);
    }

    [Fact]
    public void ReadFixedLengthString_ThrowsEndOfStreamException_WhenNotEnoughData()
    {
        var data = "AB"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadFixedLengthString(3));
    }

    [Fact]
    public void ReadFixedLengthString_ReturnsEmptyString_WhenAllNulls()
    {
        var data = new byte[] { 0, 0, 0 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadFixedLengthString(3);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadFixedLengthString_ReturnsEmptyString_WhenLengthIsZero()
    {
        using var ms = new MemoryStream([]);
        using var br = new BinaryReader(ms);

        var result = br.ReadFixedLengthString(0);

        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region ReadLine

    [Fact]
    public void ReadLine_ReturnsTextUpToLF()
    {
        var data = "Hello\n"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadLine();

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadLine_IgnoresCRBeforeLF()
    {
        var data = "Hello\r\n"u8.ToArray();
        using var ms = new MemoryStream(data);

        using var br = new BinaryReader(ms);
        var result = br.ReadLine();

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ReadLine_ReturnsEmptyString_WhenLFAtStart()
    {
        var data = "\n"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadLine();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ReadLine_ThrowsEndOfStreamException_WhenNoLF()
    {
        var data = "Hello"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadLine());
    }

    [Fact]
    public void ReadLine_ReadsConsecutiveLines()
    {
        var data = "First\nSecond\n"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var first = br.ReadLine();
        var second = br.ReadLine();

        Assert.Equal("First", first);
        Assert.Equal("Second", second);
    }

    [Fact]
    public void ReadLine_Throws_WhenStreamEndsAfterCR()
    {
        var data = "Hello\r"u8.ToArray();
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadLine());
    }

    #endregion

    #region ReadBigEndianInt24

    [Fact]
    public void ReadBigEndianInt24_ReturnsValue_WhenThreeBytes()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt24();

        Assert.Equal(0x010203, result);
    }

    [Fact]
    public void ReadBigEndianInt24_ReturnsZero_WhenAllZero()
    {
        var data = new byte[] { 0x00, 0x00, 0x00 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt24();

        Assert.Equal(0, result);
    }

    [Fact]
    public void ReadBigEndianInt24_ReturnsMaxValue_WhenAllFF()
    {
        var data = new byte[] { 0xFF, 0xFF, 0xFF };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt24();

        Assert.Equal(0xFFFFFF, result);
    }

    [Fact]
    public void ReadBigEndianInt24_ThrowsEndOfStreamException_WhenNotEnoughData()
    {
        var data = new byte[] { 0x01, 0x02 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadBigEndianInt24());
    }

    #endregion

    #region ReadBigEndianInt16

    [Fact]
    public void ReadBigEndianInt16_ReturnsValue_WhenTwoBytes()
    {
        var data = new byte[] { 0x01, 0x02 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt16();

        Assert.Equal(0x0102, result);
    }

    [Fact]
    public void ReadBigEndianInt16_ReturnsZero_WhenAllZero()
    {
        var data = new byte[] { 0x00, 0x00 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt16();

        Assert.Equal(0, result);
    }

    [Fact]
    public void ReadBigEndianInt16_ReturnsMaxValue_WhenAllFF()
    {
        var data = new byte[] { 0xFF, 0xFF };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadBigEndianInt16();

        Assert.Equal(0xFFFF, result);
    }

    [Fact]
    public void ReadBigEndianInt16_ThrowsEndOfStreamException_WhenNotEnoughData()
    {
        var data = new byte[] { 0x01 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => br.ReadBigEndianInt16());
    }

    [Fact]
    public void ReadBigEndianInt16_ReadsConsecutiveValues()
    {
        var data = new byte[] { 0x01, 0x02, 0xFF, 0xFE };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var first = br.ReadBigEndianInt16();
        var second = br.ReadBigEndianInt16();

        Assert.Equal(0x0102, first);
        Assert.Equal(0xFFFE, second);
    }

    #endregion

    #region ReadOptionalInt32

    [Fact]
    public void ReadOptionalInt32_ReturnsNull_WhenNoData()
    {
        using var ms = new MemoryStream([]);
        using var br = new BinaryReader(ms);

        var result = br.ReadOptionalInt32();

        Assert.Null(result);
    }

    [Fact]
    public void ReadOptionalInt32_ReturnsValue_WhenFourBytes()
    {
        var data = new byte[] { 1, 2, 3, 4 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var expected = BitConverter.ToInt32(data, 0);
        var result = br.ReadOptionalInt32();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ReadOptionalInt32_ThrowsArgumentException_WhenPartialBytes()
    {
        for (var len = 1; len <= 3; len++)
        {
            var data = new byte[len];
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);

            Assert.Throws<ArgumentException>(() => br.ReadOptionalInt32());
        }
    }

    #endregion

    #region ReadAllBytes

    [Fact]
    public void ReadAllBytes_ReturnsEmptyArray_WhenNoData()
    {
        using var ms = new MemoryStream([]);
        using var br = new BinaryReader(ms);

        var result = br.ReadAllBytes();

        Assert.Empty(result);
    }

    [Fact]
    public void ReadAllBytes_ReturnsAllBytes_WhenLessThanBuffer()
    {
        var data = new byte[] { 10, 20, 30, 40, 50 };
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadAllBytes();

        Assert.Equal(data, result);
    }

    [Fact]
    public void ReadAllBytes_ReturnsAllBytes_WhenExactlyBufferSize()
    {
        var data = new byte[8192];
        new Random().NextBytes(data);
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadAllBytes();

        Assert.Equal(data, result);
    }

    [Fact]
    public void ReadAllBytes_ReturnsAllBytes_WhenMultipleChunks()
    {
        var data = new byte[8192 * 3 + 100];
        new Random().NextBytes(data);
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var result = br.ReadAllBytes();

        Assert.Equal(data, result);
    }

    #endregion
}