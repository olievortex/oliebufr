using OlieBufr.Lib.Extensions;

namespace OlieBufr.Lib.Decoding;

public class IndicatorSection
{
    public string Magic { get; set; } = string.Empty;
    public int Length { get; set; }
    public int Version { get; set; }

    public static IndicatorSection Decode(BinaryReader br)
    {
        var section = new IndicatorSection
        {
            Magic = br.ReadFixedLengthString(4),
            Length = br.ReadBigEndianInt24(),
            Version = br.ReadByte()
        };

        if (section.Magic != "BUFR")
        {
            throw new ApplicationException("Not a BUFR file");
        }
        if (section.Version < 3 || section.Version > 4)
        {
            throw new ApplicationException("Not a BUFR version 3 or 4 file");
        }

        return section;
    }
}
