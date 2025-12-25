using OlieBufr.Lib.Extensions;

namespace OlieBufr.Lib.Decoding;

public class EndSection
{
    public string Magic { get; set; } = string.Empty;

    public byte[] Padding { get; set; } = [];

    public static EndSection Decode(BinaryReader br)
    {
        var magic = br.ReadFixedLengthString(4);
        var padding = br.ReadBytes(8192);

        var section = new EndSection
        {
            Magic = magic,
            Padding = padding
        };

        if (section.Magic != "7777")
        {
            throw new ApplicationException("Invalid end section magic");
        }

        return section;
    }
}
