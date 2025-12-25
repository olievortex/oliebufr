using OlieBufr.Lib.Decoding;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Tokens;
using OlieBufr.Lib.Services;

namespace OlieBufr.Lib;

public class BufrMessage
{
    public IdentificationSection Identification { get; set; } = new();
    public List<List<IBufrMessage>> SubSets { get; set; } = [];
    public List<Token> Tokens { get; set; } = [];

    public static BufrMessage Decode(byte[] frame, Dictionary<string, Element> elements, Dictionary<string, Sequence> sequences)
    {
        using var br = new BinaryReader(new MemoryStream(frame));

        var version = IndicatorSection.Decode(br).Version;
        var identification = IdentificationSection.Decode(br, version);
        var descriptor = DescriptorSection.Decode(br);
        var data = DataSection.Decode(br).Data;
        EndSection.Decode(br);

        var tokens = TokenDecoder.FromDescriptors(sequences, descriptor.Descriptors);
        var subSets = ReadSubsets(descriptor, tokens, elements, new OlieBitReader(data));

        var result = new BufrMessage()
        {
            Identification = identification,
            Tokens = tokens,
            SubSets = subSets
        };

        return result;
    }

    private static List<List<IBufrMessage>> ReadSubsets(DescriptorSection descriptor, List<Token> tokens, Dictionary<string, Element> elements, OlieBitReader data)
    {
        if (descriptor.IsCompression)
        {
            return Decompression.ReadSubsets(descriptor.Subsets, tokens, elements, data);
        }
        else
        {
            return Subsets.ReadSubsets(descriptor.Subsets, tokens, elements, data);
        }
    }
}
