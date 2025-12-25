using OlieBufr.Lib;
using OlieBufr.Lib.Models.Messages;

namespace OlieBufr.Tests;

public class BufrMessageDecodeTests
{
    [Fact]
    public void Decode_ReadsUncompressed_ValidMessage()
    {
        var bytes = MockBufr3Message.GetBytes();
        var result = BufrMessage.Decode(bytes, MockBufr3Message.Elements, MockBufr3Message.Sequences);

        Assert.NotNull(result);
        Assert.Equal("PO", ((BufrStringElement)result.SubSets[0][0]).Value);
        Assert.Equal("0-00-001", result.Tokens[0]);
    }

    [Fact]
    public void Decode_Compressed_ValidMessage()
    {
        var bytes = MockBufr4Message.GetBytes();
        var result = BufrMessage.Decode(bytes, MockBufr4Message.Elements, MockBufr4Message.Sequences);

        Assert.NotNull(result);
        Assert.Equal(10, ((BufrIntElement)result.SubSets[0][0]).Value);
        Assert.Equal(42, ((BufrIntElement)result.SubSets[0][1]).Value);
        Assert.Equal(11, ((BufrIntElement)result.SubSets[1][0]).Value);
        Assert.Equal(43, ((BufrIntElement)result.SubSets[1][1]).Value);
        Assert.Equal(25, result.Identification.Year);
    }
}