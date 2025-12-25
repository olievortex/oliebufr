using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Services;
using System.Text.Json;

namespace OlieBufr.Lib;

public class Sequences(IOlieService olieService)
{
    private static Dictionary<string, Sequence>? _reference;
    public Dictionary<string, Sequence> Reference
    {
        get
        {
            _reference ??= GetSequences(olieService);

            return _reference;
        }
    }

    public static void ClearCache()
    {
        _reference = null;
    }

    private static Dictionary<string, Sequence> GetSequences(IOlieService olieService)
    {
        var json = olieService.ReadSequencesJson();

        var sequences = JsonSerializer.Deserialize<List<Sequence>>(json) ?? [];
        sequences.ForEach(seq => seq.Bytes = [.. seq.Elements
            .Select(TokenToBytes)
            .SelectMany(b => b)]);

        return sequences.ToDictionary(seq => seq.Id, seq => seq);
    }

    public static byte[] TokenToBytes(string token)
    {
        var bytes = new byte[2];

        bytes[0] = (byte)(token[0] << 6);
        bytes[0] |= (byte)int.Parse(token[2..4]);
        bytes[1] = (byte)int.Parse(token[5..8]);

        return bytes;
    }
}
