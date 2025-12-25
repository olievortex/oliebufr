using OlieBufr.Lib.Extensions;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Tokens;

namespace OlieBufr.Lib.Decoding;

public static class TokenDecoder
{
    public static List<Token> FromDescriptors(Dictionary<string, Sequence> sequences, byte[] descriptors)
    {
        using var descriptor = new BinaryReader(new MemoryStream(descriptors));

        var end = descriptors.Length / 2 * 2;
        var result = new List<Token>();

        while (descriptor.BaseStream.Position < end)
        {
            result.AddRange(DecodeTokens(sequences, descriptor));
        }

        return result;
    }

    public static List<Token> DecodeTokens(Dictionary<string, Sequence> sequences, BinaryReader descriptor)
    {
        var token = ReadToken(descriptor);

        return token.F switch
        {
            0 => [token],
            1 => [DecodeReplicationToken(token, sequences, descriptor)],
            2 => [token],
            _ => DecodeSequenceToken(token, sequences),
        };
    }

    public static Token DecodeReplicationToken(Token token, Dictionary<string, Sequence> sequences, BinaryReader descriptor)
    {
        Token? delayedToken = null;

        if (token.Y == 0)
        {
            delayedToken = ReadToken(descriptor);
            if (delayedToken != "0-31-001" && delayedToken != "0-31-002")
            {
                throw new NotSupportedException("Expected BufrReplicationLengthByte for delayed replication.");
            }
        }

        var tokens = new List<Token>();
        var work = descriptor.ReadRequired(token.X * 2);
        var workload = new BinaryReader(new MemoryStream(work));
        var end = work.Length / 2 * 2;

        while (workload.BaseStream.Position < end)
        {
            tokens.AddRange(DecodeTokens(sequences, workload));
        }

        var result = new ReplicationToken(token)
        {
            DelayedToken = delayedToken,
            Tokens = tokens
        };

        return result;
    }

    public static List<Token> DecodeSequenceToken(string token, Dictionary<string, Sequence> sequences)
    {
        if (!sequences.TryGetValue(token, out var sequence))
            throw new NotSupportedException($"Element descriptor {token} is not supported.");

        var elements = new List<Token>();
        var workload = new BinaryReader(new MemoryStream(sequence.Bytes));
        var end = sequence.Bytes.Length / 2 * 2;

        while (workload.BaseStream.Position < end)
        {
            elements.AddRange(DecodeTokens(sequences, workload));
        }

        return elements;
    }

    public static Token ReadToken(BinaryReader br)
    {
        var bytes = br.ReadBytes(2);

        if (bytes.Length < 2)
        {
            throw new EndOfStreamException("Could not read enough bytes for Descriptors");
        }

        var f = (bytes[0] & 0xC0) >>> 6;
        var x = bytes[0] & 0x3F;
        var y = bytes[1];

        return new Token(f, x, y);
    }
}
