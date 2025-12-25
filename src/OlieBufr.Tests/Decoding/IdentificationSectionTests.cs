using OlieBufr.Lib.Decoding;

namespace OlieBufr.Tests.Decoding;

public class IdentificationSectionTests
{
    [Fact]
    public void Decode3_ReturnsExpectedValues_WhenNoSection2()
    {
        // length = 17 (0x00 0x00 0x11)
        var bytes = new byte[]
        {
            0x00, 0x00, 0x11, // length
            0x01, // masterTable
            0x02, // originatingSubCenter
            0x03, // originatingCenter
            0x04, // sequence
            0x00, // hasSection2
            0x05, // category
            0x06, // subCategory
            0x07, // masterTableVersion
            0x08, // localTableVersion
            0x15, // year (21)
            0x0C, // month (12)
            0x1F, // day (31)
            0x17, // hour (23)
            0x3B  // minute (59)
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        var section = IdentificationSection.Decode(br, 3);

        Assert.Equal(17, section.Length);
        Assert.Equal(1, section.MasterTable);
        Assert.Equal(3, section.OriginatingCenter);
        Assert.Equal(2, section.OriginatingSubCenter);
        Assert.Equal(4, section.Sequence);
        Assert.False(section.HasSection2);
        Assert.Equal(5, section.Category);
        Assert.Equal(6, section.SubCategory);
        Assert.Equal(7, section.MasterTableVersion);
        Assert.Equal(8, section.LocalTableVersion);
        Assert.Equal(21, section.Year);
        Assert.Equal(12, section.Month);
        Assert.Equal(31, section.Day);
        Assert.Equal(23, section.Hour);
        Assert.Equal(59, section.Minute);
        Assert.Empty(section.Data2);
    }

    [Fact]
    public void Decode3_ReadsData2_WhenLengthGreaterThan17()
    {
        // length = 20 (0x00 0x00 0x14) -> 3 bytes of Data2
        var bytes = new byte[]
        {
            0x00, 0x00, 0x14, // length
            0x01, // masterTable
            0x02, // originatingSubCenter
            0x03, // originatingCenter
            0x04, // sequence
            0x00, // hasSection2
            0x05, // category
            0x06, // subCategory
            0x07, // masterTableVersion
            0x08, // localTableVersion
            0x15, // year
            0x0C, // month
            0x1F, // day
            0x17, // hour
            0x3B, // minute
            0x09, 0x0A, 0x0B // Data2 (3 bytes)
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        var section = IdentificationSection.Decode(br, 3);

        Assert.Equal(20, section.Length);
        Assert.Equal(new byte[] { 0x09, 0x0A, 0x0B }, section.Data2);
    }

    [Fact]
    public void Decode3_ThrowsNotSupported_WhenSection2Present()
    {
        var bytes = new byte[]
        {
            0x00, 0x00, 0x11, // length
            0x01, // masterTable
            0x02, // originatingSubCenter
            0x03, // originatingCenter
            0x04, // sequence
            0x01, // hasSection2 = true
            0x05, // category
            0x06, // subCategory
            0x07, // masterTableVersion
            0x08, // localTableVersion
            0x15, // year
            0x0C, // month
            0x1F, // day
            0x17, // hour
            0x3B  // minute
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        Assert.Throws<NotSupportedException>(() => IdentificationSection.Decode(br, 3));
    }

    [Fact]
    public void Decode4_ReturnsExpectedValues_WhenNoSection2()
    {
        // length = 22 (0x00 0x00 0x16)
        var bytes = new byte[]
        {
            0x00, 0x00, 0x16, // length
            0x01, // masterTable
            0x01, 0x02, // originatingSubCenter (0x0102 -> 258)
            0x03, 0x04, // originatingCenter (0x0304 -> 772)
            0x05, // sequence
            0x00, // hasSection2
            0x06, // category
            0x07, // subCategory
            0x08, // localSubCategory
            0x09, // masterTableVersion
            0x0A, // localTableVersion
            0x07, 0xE4, // year (2020)
            0x0C, // month
            0x1F, // day
            0x17, // hour
            0x3B, // minute
            0x3A  // second (58)
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        var section = IdentificationSection.Decode(br, 4);

        Assert.Equal(22, section.Length);
        Assert.Equal(1, section.MasterTable);
        Assert.Equal(772, section.OriginatingCenter);
        Assert.Equal(258, section.OriginatingSubCenter);
        Assert.Equal(5, section.Sequence);
        Assert.False(section.HasSection2);
        Assert.Equal(6, section.Category);
        Assert.Equal(7, section.SubCategory);
        Assert.Equal(8, section.LocalSubCategory);
        Assert.Equal(9, section.MasterTableVersion);
        Assert.Equal(10, section.LocalTableVersion);
        Assert.Equal(2020, section.Year);
        Assert.Equal(12, section.Month);
        Assert.Equal(31, section.Day);
        Assert.Equal(23, section.Hour);
        Assert.Equal(59, section.Minute);
        Assert.Equal(58, section.Second);
        Assert.Empty(section.Data2);
    }

    [Fact]
    public void Decode4_ReadsData2_WhenLengthGreaterThan22()
    {
        // length = 25 (0x00 0x00 0x19) -> 3 bytes Data2
        var bytes = new byte[]
        {
            0x00, 0x00, 0x19, // length
            0x01, // masterTable
            0x01, 0x02, // originatingSubCenter
            0x03, 0x04, // originatingCenter
            0x05, // sequence
            0x00, // hasSection2
            0x06, // category
            0x07, // subCategory
            0x08, // localSubCategory
            0x09, // masterTableVersion
            0x0A, // localTableVersion
            0x07, 0xE4, // year
            0x0C, // month
            0x1F, // day
            0x17, // hour
            0x3B, // minute
            0x3A, // second
            0x63, 0x64, 0x65 // Data2
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        var section = IdentificationSection.Decode(br, 4);

        Assert.Equal(25, section.Length);
        Assert.Equal("cde"u8.ToArray(), section.Data2);
    }

    [Fact]
    public void Decode4_ThrowsNotSupported_WhenSection2Present()
    {
        var bytes = new byte[]
        {
            0x00, 0x00, 0x16, // length
            0x01, // masterTable
            0x01, 0x02, // originatingSubCenter
            0x03, 0x04, // originatingCenter
            0x05, // sequence
            0x01, // hasSection2 = true
            0x06, // category
            0x07, // subCategory
            0x08, // localSubCategory
            0x09, // masterTableVersion
            0x0A, // localTableVersion
            0x07, 0xE4, // year
            0x0C, // month
            0x1F, // day
            0x17, // hour
            0x3B, // minute
            0x3A  // second
        };

        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        Assert.Throws<NotSupportedException>(() => IdentificationSection.Decode(br, 4));
    }

    [Fact]
    public void Decode_ThrowsNotImplementedException_UnsupportedVersion()
    {
        Assert.Throws<NotImplementedException>(() => IdentificationSection.Decode(null!, 6));
    }
}