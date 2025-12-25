using OlieBufr.Lib.Extensions;
using System.Text;

namespace OlieBufr.Lib.Decoding;

public class FrameSection
{
    public int Length { get; set; }
    public byte[] Data { get; set; } = [];
    public int Checksum { get; set; }
    public string Description { get; set; } = string.Empty;

    public static FrameSection? Decode(BinaryReader br)
    {
        var length = br.ReadOptionalInt32();
        if (length is null) return null;
        if (length == 1314084169) return DecodeFrame4(br);

        var section = new FrameSection
        {
            Length = length.Value,
            Data = br.ReadRequired(length.Value),
            Checksum = br.ReadInt32()
        };

        if (section.Length != section.Checksum)
        {
            throw new InvalidDataException($"Section 3 checksum mismatch: Length={section.Length}, Checksum={section.Checksum}");
        }

        return section;
    }

    private static FrameSection DecodeFrame4(BinaryReader br)
    {
        var sw = new StringBuilder("IUSN");

        sw.AppendLine(br.ReadLine());
        sw.AppendLine(br.ReadLine());

        var message = br.ReadAllBytes();

        var data = message[..^4].ToArray();
        var checksum = message[^4..].ToArray();

        var result = new FrameSection
        {
            Length = data.Length,
            Checksum = BitConverter.ToInt32(checksum, 0),
            Data = data,
            Description = sw.ToString()
        };

        if (result.Checksum != 1313754702) throw new ApplicationException("Unexpected checksum");

        return result;
    }
}
