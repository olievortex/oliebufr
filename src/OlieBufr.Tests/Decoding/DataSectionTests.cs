using OlieBufr.Lib.Decoding;

namespace OlieBufr.Tests.Decoding;

public class DataSectionTests
{
    [Fact]
    public void Decode_ReturnsSection_WhenValidData()
    {
        var payload = new byte[] { 0x00, 0x00, 0x07, 0x7A, 10, 20, 30 };
        using var br = new BinaryReader(new MemoryStream(payload));

        var section = DataSection.Decode(br);

        Assert.Equal(7, section.Length);
        Assert.Equal(0x7a, section.Padding);
        Assert.Equal([10, 20, 30], section.Data);
    }

    [Fact]
    public void Decode_ReturnsEmptyData_WhenLengthIsFour()
    {
        var payload = new byte[] { 0x00, 0x00, 0x04, 0x7A };
        using var br = new BinaryReader(new MemoryStream(payload));

        var section = DataSection.Decode(br);

        Assert.Equal(4, section.Length);
        Assert.Equal(0x7A, section.Padding);
        Assert.Empty(section.Data);
    }

    [Fact]
    public void Decode_ThrowsEndOfStreamException_WhenNotEnoughDataForData()
    {
        var payload = new byte[] { 0x00, 0x00, 0x07, 0x7A, 10, 20 };
        using var br = new BinaryReader(new MemoryStream(payload));

        Assert.Throws<EndOfStreamException>(() => DataSection.Decode(br));
    }

    [Fact]
    public void Decode_ThrowsEndOfStreamException_WhenNotEnoughBytesForLength()
    {
        var payload = new byte[] { 0x01, 0x02 }; // 2 bytes only
        using var br = new BinaryReader(new MemoryStream(payload));

        Assert.Throws<EndOfStreamException>(() => DataSection.Decode(br));
    }
}