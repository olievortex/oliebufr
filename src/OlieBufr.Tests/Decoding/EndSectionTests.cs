using OlieBufr.Lib.Decoding;
using System.Text;

namespace OlieBufr.Tests.Decoding;

public class EndSectionTests
{
    [Fact]
    public void Decode_ReturnsSection_WhenMagicIs7777()
    {
        var magicBytes = Encoding.ASCII.GetBytes("7777");
        var padding = new byte[8192];
        for (var i = 0; i < padding.Length; i++)
        {
            padding[i] = (byte)(i & 0xFF);
        }

        var data = new byte[magicBytes.Length + padding.Length];
        Buffer.BlockCopy(magicBytes, 0, data, 0, magicBytes.Length);
        Buffer.BlockCopy(padding, 0, data, magicBytes.Length, padding.Length);

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var section = EndSection.Decode(br);

        Assert.Equal("7777", section.Magic);
        Assert.Equal(padding.Length, section.Padding.Length);
        Assert.Equal(padding, section.Padding);
    }

    [Fact]
    public void Decode_ThrowsApplicationException_WhenMagicInvalid()
    {
        var magicBytes = Encoding.ASCII.GetBytes("0000");
        var padding = new byte[8192];
        var data = new byte[magicBytes.Length + padding.Length];
        Buffer.BlockCopy(magicBytes, 0, data, 0, magicBytes.Length);
        Buffer.BlockCopy(padding, 0, data, magicBytes.Length, padding.Length);

        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        var ex = Assert.Throws<ApplicationException>(() => EndSection.Decode(br));
        Assert.Contains("Invalid end section magic", ex.Message);
    }

    [Fact]
    public void Decode_ThrowsEndOfStreamException_WhenNotEnoughData()
    {
        var shortData = "777"u8.ToArray(); // less than 4 bytes for magic
        using var ms = new MemoryStream(shortData);
        using var br = new BinaryReader(ms);

        Assert.Throws<EndOfStreamException>(() => EndSection.Decode(br));
    }
}
