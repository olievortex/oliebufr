using OlieBufr.Lib.Extensions;

namespace OlieBufr.Lib.Decoding;

public class DescriptorSection
{
    public int Length { get; set; }
    public int Padding { get; set; }
    public int Subsets { get; set; }
    public int Flags { get; set; }
    public bool IsCompression { get; set; }
    public byte[] Descriptors { get; set; } = [];

    public static DescriptorSection Decode(BinaryReader br)
    {
        var length = br.ReadBigEndianInt24();
        var padding = br.ReadByte();
        var subsets = br.ReadBigEndianInt16();
        var flags = br.ReadByte();
        var descriptors = br.ReadRequired(length - 7);

        var result = new DescriptorSection
        {
            Length = length,
            Padding = padding,
            Subsets = subsets,
            Flags = flags,
            Descriptors = descriptors,
            IsCompression = (flags & 0x40) != 0
        };

        return result;
    }
}
