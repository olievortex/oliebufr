using OlieBufr.Lib.Models;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Messages.Operation;
using OlieBufr.Lib.Models.Tokens;
using OlieBufr.Lib.Services;

namespace OlieBufr.Lib.Decoding;

public static class Subsets
{
    public static List<List<IBufrMessage>> ReadSubsets(int subsets, List<Token> tokens, Dictionary<string, Element> elements, OlieBitReader data)
    {
        var result = new List<List<IBufrMessage>>();

        for (var i = 0; i < subsets; i++)
        {
            result.Add(ReadMessages(tokens, elements, data));
        }

        return result;
    }

    public static List<IBufrMessage> ReadMessages(List<Token> tokens, Dictionary<string, Element> elements, OlieBitReader data)
    {
        var result = new List<IBufrMessage>();
        var state = new OperationElementState();

        foreach (var token in tokens)
        {
            result.Add(ReadMessage(token, elements, state, data));
        }

        return result;
    }

    public static IBufrMessage ReadMessage(Token token, Dictionary<string, Element> elements, OperationElementState state, OlieBitReader data)
    {
        return token.F switch
        {
            0 => ReadElement(token, elements, state, data),
            1 => ReadReplicated((ReplicationToken)token, elements, data),
            2 => ReadOperation(token, state, data),
            _ => throw new NotSupportedException($"Descriptor {token} is not supported."),
        };
    }

    public static IBufrMessage ReadElement(Token token, Dictionary<string, Element> elements, OperationElementState state, OlieBitReader data)
    {
        if (!elements.TryGetValue(token, out var descriptor))
            throw new NotSupportedException($"Element descriptor {token} is not supported.");

        return descriptor.Type switch
        {
            ElementTypesEnum.Table => new BufrIntElement
            {
                Element = descriptor,
                Value = data.ReadBits(descriptor.Width),
            },
            ElementTypesEnum.String => new BufrStringElement
            {
                Element = descriptor,
                Value = data.ReadFixedLengthString(descriptor.Width / 8)
            },
            ElementTypesEnum.Number => new BufrNumberElement
            {
                Element = descriptor,
                Value = InterpretBits(descriptor, state, data),
            },
            _ => throw new InvalidOperationException(descriptor.Type.ToString()),
        };
    }

    public static double InterpretBits(Element element, OperationElementState state, OlieBitReader data)
    {
        var bits = data.ReadBits(element.Width + state.Width);
        var missing = (1 << (element.Width + state.Width)) - 1;

        if (bits == missing)
        {
            return double.NaN;
        }

        return Math.Pow(10, -(element.Scale + state.Scale)) * (element.Offset + bits);
    }

    public static IBufrMessage ReadOperation(Token token, OperationElementState state, OlieBitReader data)
    {
        switch (token.X)
        {
            case 1:
                var dataWidthOperation = new ChangeDataWidthOperation(token.Y);
                state.Width = dataWidthOperation.Offset;
                return dataWidthOperation;
            case 2:
                var changeScaleOperation = new ChangeScaleOperation(token.Y);
                state.Scale = changeScaleOperation.Offset;
                return changeScaleOperation;
            case 5:
                return new SignifyCharacterOperation(token.Y, data);
            default:
                throw new NotSupportedException($"Descriptor {token} is not supported.");
        }
    }

    public static IBufrMessage ReadReplicated(ReplicationToken token, Dictionary<string, Element> elements, OlieBitReader data)
    {
        var result = new BufrReplication();
        var count = token.Y;

        // Delayed replication, read count from data section
        if (count == 0)
        {
            if (token.DelayedToken is null || (token.DelayedToken != "0-31-001" && token.DelayedToken != "0-31-002"))
            {
                throw new NotSupportedException("Expected BufrReplicationLengthByte for delayed replication.");
            }

            if (token.DelayedToken == "0-31-001")
            {
                count = data.ReadByte();
            }
            else
            {
                count = data.ReadBits(16);
            }
        }

        for (var i = 0; i < count; i++)
        {
            result.Messages.AddRange(ReadMessages(token.Tokens, elements, data));
        }

        return result;
    }
}
