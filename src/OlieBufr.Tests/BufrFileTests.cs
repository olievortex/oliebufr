using OlieBufr.Lib;

namespace OlieBufr.Tests;

public class BufrFileTests
{
    [Fact]
    public void Decode_Decodes_File()
    {
        var message = MockBufr4Message.GetBytes();
        var length = BitConverter.GetBytes(message.Length);
        var file = new List<byte[]> { length, message, length };
        var bytes = file.SelectMany(b => b).ToArray();
        using var br = new BinaryReader(new MemoryStream(bytes));

        var result = BufrFile.Decode(MockBufr4Message.Elements, MockBufr4Message.Sequences, br);

        Assert.Single(result.BufrMessages);
    }
}
