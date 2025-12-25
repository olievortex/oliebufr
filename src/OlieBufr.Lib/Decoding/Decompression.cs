using OlieBufr.Lib.Models;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Messages.Operation;
using OlieBufr.Lib.Models.Tokens;
using OlieBufr.Lib.Services;

namespace OlieBufr.Lib.Decoding;

public static class Decompression
{
    public static List<List<IBufrMessage>> ReadSubsets(int subsets, List<Token> tokens, Dictionary<string, Element> elements, OlieBitReader data)
    {
        var decoded = DecodeValues(subsets, tokens, elements, data);
        var result = new List<List<IBufrMessage>>();

        for (var j = 0; j < subsets; j++)
        {
            var messages = new List<IBufrMessage>();

            for (var i = 0; i < tokens.Count; i++)
            {
                if (decoded[i] != null)
                {
                    messages.Add(decoded[i][j]);
                }
            }

            result.Add(messages);
        }

        return result;
    }

    public static List<IBufrMessage>[] DecodeValues(int subsets, List<Token> tokens, Dictionary<string, Element> elements, OlieBitReader data)
    {
        var state = new OperationElementState();
        var flattenedTokens = ApplyReplication(tokens);
        var premia = new List<IBufrMessage>[flattenedTokens.Count];
        var premiaPtr = -1;

        foreach (var token in flattenedTokens)
        {
            premiaPtr++;

            if (token.F == 2)
            {
                switch (token.X)
                {
                    case 1:
                        var dataWidthOperation = new ChangeDataWidthOperation(token.Y);
                        state.Width = dataWidthOperation.Offset;
                        break;
                    case 2:
                        var changeScaleOperation = new ChangeScaleOperation(token.Y);
                        state.Scale = changeScaleOperation.Offset;
                        break;
                    default:
                        throw new NotSupportedException($"Descriptor {token} is not supported.");
                }

                continue;
            }
            else if (token.F == 1) throw new NotImplementedException();

            if (!elements.TryGetValue(token, out var element))
                throw new NotSupportedException($"Element descriptor {token} is not supported.");

            premia[premiaPtr] = ApplyIncrementToElement(subsets, element, state, data);
        }

        return premia;
    }

    private static List<Token> ApplyReplication(List<Token> tokens)
    {
        var result = new List<Token>();

        foreach (var token in tokens)
        {
            if (token is ReplicationToken replicationToken)
            {
                if (replicationToken.Y == 0) throw new NotImplementedException();

                for (var i = 0; i < replicationToken.Y; i++)
                {
                    result.AddRange(replicationToken.Tokens);
                }
            }
            else
            {
                result.Add(token);
            }
        }

        return result;
    }

    private static List<IBufrMessage> ApplyIncrementToElement(int subsets, Element element, OperationElementState oeState, OlieBitReader data)
    {
        return element.Type switch
        {
            ElementTypesEnum.Table => ApplyIncrementToTableElement(subsets, element, data),
            ElementTypesEnum.Number => ApplyIncrementToNumberElement(subsets, element, oeState, data),
            _ => throw new NotImplementedException(),
        };
    }

    private static List<IBufrMessage> ApplyIncrementToNumberElement(int subsets, Element element, OperationElementState oeState, OlieBitReader data)
    {
        var scale = Math.Pow(10, -(element.Scale + oeState.Scale));
        var localValue = data.ReadBits(element.Width + oeState.Width);
        var missing = (1 << (element.Width + oeState.Width)) - 1;
        var increments = data.ReadBits(6);
        var result = new List<IBufrMessage>();

        for (var i = 0; i < subsets; i++)
        {
            var increment = increments == 0 ? 0 : data.ReadBits(increments);
            var baseValue = localValue + increment;
            var value = (element.Offset + baseValue) * scale;

            var message = new BufrNumberElement
            {
                Element = element,
                Value = baseValue == missing ? double.NaN : value
            };

            result.Add(message);
        }

        return result;
    }

    private static List<IBufrMessage> ApplyIncrementToTableElement(int subsets, Element element, OlieBitReader data)
    {
        if (element.Scale != 0 || element.Offset != 0) throw new InvalidOperationException();

        var baseValue = element.Offset + data.ReadBits(element.Width);
        var increments = data.ReadBits(6);
        var result = new List<IBufrMessage>();

        for (var i = 0; i < subsets; i++)
        {
            var increment = increments == 0 ? 0 : data.ReadBits(increments);
            var value = baseValue + increment;

            var message = new BufrIntElement
            {
                Element = element,
                Value = value
            };

            result.Add(message);
        }

        return result;
    }
}
