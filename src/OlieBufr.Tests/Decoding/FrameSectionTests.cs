using OlieBufr.Lib.Decoding;
using System.Text;

namespace OlieBufr.Tests.Decoding;

public class FrameSectionTests
{
    [Fact]
    public void Decode_ReturnsNull_WhenNoData()
    {
        using var ms = new MemoryStream([]);
        using var br = new BinaryReader(ms);

        var result = FrameSection.Decode(br);

        Assert.Null(result);
    }

    [Fact]
    public void Decode_ReturnsSection_WhenLengthAndChecksumMatch()
    {
        var length = 5;
        var data = new byte[] { 1, 2, 3, 4, 5 };

        using var ms = new MemoryStream();
        ms.Write(BitConverter.GetBytes(length));
        ms.Write(data);
        ms.Write(BitConverter.GetBytes(length)); // checksum matches length
        ms.Position = 0;

        using var br = new BinaryReader(ms);
        var result = FrameSection.Decode(br);

        Assert.NotNull(result);
        Assert.Equal(length, result.Length);
        Assert.Equal(length, result.Checksum);
        Assert.Equal(data, result.Data);
    }

    [Fact]
    public void Decode_ThrowsInvalidDataException_WhenChecksumMismatch()
    {
        var length = 3;
        var data = new byte[] { 10, 20, 30 };
        var badChecksum = 999;

        using var ms = new MemoryStream();
        ms.Write(BitConverter.GetBytes(length));
        ms.Write(data);
        ms.Write(BitConverter.GetBytes(badChecksum));
        ms.Position = 0;

        using var br = new BinaryReader(ms);

        var ex = Assert.Throws<InvalidDataException>(() => FrameSection.Decode(br));
        Assert.Contains(length.ToString(), ex.Message);
        Assert.Contains(badChecksum.ToString(), ex.Message);
    }

    [Fact]
    public void DecodeFrame4_ReturnsSection_WhenMagicAndValidChecksum()
    {
        const int magic = 1314084169;
        const int expectedChecksum = 1313754702;
        var header1 = "HeaderOne";
        var header2 = "HeaderTwo";
        var payload = new byte[] { 5, 6, 7 };

        using var ms = new MemoryStream();
        ms.Write(BitConverter.GetBytes(magic));
        ms.Write(Encoding.ASCII.GetBytes(header1 + "\n"));
        ms.Write(Encoding.ASCII.GetBytes(header2 + "\n"));
        ms.Write(payload);
        ms.Write(BitConverter.GetBytes(expectedChecksum));
        ms.Position = 0;

        using var br = new BinaryReader(ms);
        var result = FrameSection.Decode(br);

        Assert.NotNull(result);
        Assert.Equal(expectedChecksum, result.Checksum);
        Assert.Equal(payload.Length, result.Length);
        Assert.Equal(payload, result.Data);
        Assert.StartsWith("IUSN", result.Description);
        Assert.Contains(header1, result.Description);
        Assert.Contains(header2, result.Description);
    }

    [Fact]
    public void DecodeFrame4_ThrowsApplicationException_WhenUnexpectedChecksum()
    {
        const int magic = 1314084169;
        const int wrongChecksum = 123456789;
        var header1 = "H1";
        var header2 = "H2";
        var payload = new byte[] { 9, 8, 7, 6 };

        using var ms = new MemoryStream();
        ms.Write(BitConverter.GetBytes(magic));
        ms.Write(Encoding.ASCII.GetBytes(header1 + "\n"));
        ms.Write(Encoding.ASCII.GetBytes(header2 + "\n"));
        ms.Write(payload);
        ms.Write(BitConverter.GetBytes(wrongChecksum));
        ms.Position = 0;

        using var br = new BinaryReader(ms);
        Assert.Throws<ApplicationException>(() => FrameSection.Decode(br));
    }
}
