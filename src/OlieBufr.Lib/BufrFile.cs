using OlieBufr.Lib.Decoding;
using OlieBufr.Lib.Models.Messages;

namespace OlieBufr.Lib;

public class BufrFile
{
    public List<BufrMessage> BufrMessages { get; set; } = [];

    public static BufrFile Decode(Dictionary<string, Element> elements, Dictionary<string, Sequence> sequences, BinaryReader br)
    {
        var messages = new List<BufrMessage>();

        while (true)
        {
            var frame = FrameSection.Decode(br);
            if (frame is null) break;

            var v3 = BufrMessage.Decode(frame.Data, elements, sequences);

            messages.Add(v3);
        }

        return new BufrFile
        {
            BufrMessages = messages,
        };
    }
}
