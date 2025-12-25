using OlieBufr.Lib.Extensions;

namespace OlieBufr.Lib.Decoding;

public class IdentificationSection
{
    public int Length { get; set; }
    public int MasterTable { get; set; }
    public int OriginatingCenter { get; set; }
    public int OriginatingSubCenter { get; set; }
    public int Sequence { get; set; }
    public int Category { get; set; }
    public int SubCategory { get; set; }
    public int LocalSubCategory { get; set; }
    public int MasterTableVersion { get; set; }
    public int LocalTableVersion { get; set; }
    public byte[] Data2 { get; set; } = [];
    public bool HasSection2 { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
    public int Second { get; set; }

    public static IdentificationSection Decode(BinaryReader br, int version)
    {
        if (version == 3) return Decode3(br);
        if (version == 4) return Decode4(br);
        throw new NotImplementedException();
    }

    private static IdentificationSection Decode3(BinaryReader br)
    {
        var length = br.ReadBigEndianInt24();
        var masterTable = br.ReadByte();
        var originatingSubCenter = br.ReadByte();
        var originatingCenter = br.ReadByte();
        var sequence = br.ReadByte();
        var hasSection2 = br.ReadByte();
        var category = br.ReadByte();
        var subCategory = br.ReadByte();
        var masterTableVersion = br.ReadByte();
        var localTableVersion = br.ReadByte();
        var year = br.ReadByte();
        var month = br.ReadByte();
        var day = br.ReadByte();
        var hour = br.ReadByte();
        var minute = br.ReadByte();
        var data2 = br.ReadRequired(length - 17);

        var section = new IdentificationSection
        {
            Length = length,
            MasterTable = masterTable,
            OriginatingCenter = originatingCenter,
            OriginatingSubCenter = originatingSubCenter,
            Sequence = sequence,
            HasSection2 = hasSection2 != 0,
            Category = category,
            SubCategory = subCategory,
            MasterTableVersion = masterTableVersion,
            LocalTableVersion = localTableVersion,
            Data2 = data2,
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Minute = minute
        };

        if (section.HasSection2)
        {
            throw new NotSupportedException("BUFR messages with Section 2 are not supported.");
        }

        return section;
    }

    private static IdentificationSection Decode4(BinaryReader br)
    {
        var length = br.ReadBigEndianInt24();
        var masterTable = br.ReadByte();
        var originatingSubCenter = br.ReadBigEndianInt16();
        var originatingCenter = br.ReadBigEndianInt16();
        var sequence = br.ReadByte();
        var hasSection2 = br.ReadByte();
        var category = br.ReadByte();
        var subCategory = br.ReadByte();
        var localSubCategory = br.ReadByte();
        var masterTableVersion = br.ReadByte();
        var localTableVersion = br.ReadByte();
        var year = br.ReadBigEndianInt16();
        var month = br.ReadByte();
        var day = br.ReadByte();
        var hour = br.ReadByte();
        var minute = br.ReadByte();
        var second = br.ReadByte();
        var data2 = br.ReadRequired(length - 22);

        var section = new IdentificationSection
        {
            Length = length,
            MasterTable = masterTable,
            OriginatingCenter = originatingCenter,
            OriginatingSubCenter = originatingSubCenter,
            Sequence = sequence,
            HasSection2 = hasSection2 != 0,
            Category = category,
            SubCategory = subCategory,
            LocalSubCategory = localSubCategory,
            MasterTableVersion = masterTableVersion,
            LocalTableVersion = localTableVersion,
            Data2 = data2,
            Year = year,
            Month = month,
            Day = day,
            Hour = hour,
            Minute = minute,
            Second = second
        };

        if (section.HasSection2)
        {
            throw new NotSupportedException("BUFR messages with Section 2 are not supported.");
        }

        return section;
    }
}
