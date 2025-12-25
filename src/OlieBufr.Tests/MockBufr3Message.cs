using OlieBufr.Lib.Models.Messages;

namespace OlieBufr.Tests;

public static class MockBufr3Message
{
    public static Dictionary<string, Element> Elements { get; } = new()
    {
        { "0-00-001", new Element() { Type = ElementTypesEnum.String, Width = 16 } }
    };

    public static Dictionary<string, Sequence> Sequences { get; } = [];

    public static byte[] GetBytes()
    {
        var indicator = new List<byte> { (byte)'B', (byte)'U', (byte)'F', (byte)'R', 0, 0, 0, 3 };
        var identification = new List<byte> { 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 12, 24, 15, 47, 0 };
        var descriptor = new List<byte> { 0, 0, 10, 0, 0, 1, 0x80, 0, 1, 0 };
        var data = new List<byte> { 0, 0, 6, 0, (byte)'P', (byte)'O' };
        var end = new List<byte> { (byte)'7', (byte)'7', (byte)'7', (byte)'7' };
        var sections = new List<List<byte>> { indicator, identification, descriptor, data, end };
        var file = sections.Select(s => s.ToArray()).SelectMany(s => s).ToArray();

        return file;
    }
}
