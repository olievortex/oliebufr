using OlieBufr.Lib.Extensions;

namespace OlieBufr.Lib.Decoding;

public class DataSection
{
    public int Length { get; set; }
    public int Padding { get; set; }
    public byte[] Data { get; set; } = [];

    public static DataSection Decode(BinaryReader br)
    {
        var length = br.ReadBigEndianInt24();
        var padding = br.ReadByte();
        var section = new DataSection
        {
            Length = length,
            Padding = padding,
            Data = br.ReadRequired(length - 4)
        };

        return section;
    }
}
