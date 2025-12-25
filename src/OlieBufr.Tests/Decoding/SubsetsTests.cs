using OlieBufr.Lib.Decoding;
using OlieBufr.Lib.Models;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Messages.Operation;
using OlieBufr.Lib.Models.Tokens;
using OlieBufr.Lib.Services;
using System.Text;

namespace OlieBufr.Tests.Decoding;

public class SubsetsTests
{
    #region ReadReplicated

    [Fact]
    public void ReadReplicated_ThrowsNotSupportedException_InvalidDelayedToken()
    {
        var token = new ReplicationToken(new Token(1, 3, 0))
        {
            DelayedToken = new Token(0, 31, 3)
        };
        var obr = new OlieBitReader([]);

        Assert.Throws<NotSupportedException>(() => Subsets.ReadReplicated(token, [], obr));
    }

    [Fact]
    public void ReadReplicated_ThrowsNotSupportedException_NullDelayedToken()
    {
        var token = new ReplicationToken(new Token(1, 3, 0));
        var obr = new OlieBitReader([]);

        Assert.Throws<NotSupportedException>(() => Subsets.ReadReplicated(token, [], obr));
    }

    [Fact]
    public void ReadReplicated_ReturnsMessages_ByteDelayedToken()
    {
        var token = new ReplicationToken(new Token(1, 3, 0))
        {
            DelayedToken = new Token(0, 31, 1),
            Tokens = [new(0, 0, 1), new(0, 0, 2)]
        };
        var data = new List<byte> { 4, (byte)'a', 1, (byte)'b', 2, (byte)'c', 3, (byte)'d', 4 }.ToArray();
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-00-001", new Element() { Width = 8, Type = ElementTypesEnum.String } },
            { "0-00-002", new Element() { Width = 8, Type = ElementTypesEnum.Table, Description = "Peggy", Name = "Su" } }
        };

        var result = (BufrReplication)Subsets.ReadReplicated(token, elements, obr);

        Assert.Equal(4, result.Messages.Count);
        Assert.Equal("a", ((BufrStringElement)result.Messages[0][0]).Value);
        Assert.Equal("d", ((BufrStringElement)result.Messages[3][0]).Value);
        Assert.Equal(1, ((BufrIntElement)result.Messages[0][1]).Value);
        Assert.Equal(4, ((BufrIntElement)result.Messages[3][1]).Value);
        Assert.Equal("Peggy", ((BufrIntElement)result.Messages[1][1]).Element.Description);
        Assert.Equal("Su: 2", ((BufrIntElement)result.Messages[1][1]).ToString());
    }

    [Fact]
    public void ReadReplicated_ReturnsMessages_WordDelayedToken()
    {
        var token = new ReplicationToken(new Token(1, 3, 0))
        {
            DelayedToken = new Token(0, 31, 2),
            Tokens = [new(0, 0, 1), new(0, 0, 2)]
        };
        var data = new List<byte> { 0, 4, (byte)'a', 1, (byte)'b', 2, (byte)'c', 0xff, (byte)'d', 4 }.ToArray();
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-00-001", new Element() { Width = 8, Type = ElementTypesEnum.String, Name = "Dm", Units = "Km" } },
            { "0-00-002", new Element() { Width = 8, Type = ElementTypesEnum.Number, Scale = 2, Offset = 42, Name = "Po" } }
        };

        var result = (BufrReplication)Subsets.ReadReplicated(token, elements, obr);

        Assert.Equal(4, result.Messages.Count);
        Assert.Equal("Count = 4", result.ToString());
        Assert.Equal("a", ((BufrStringElement)result.Messages[0][0]).Value);
        Assert.Equal("d", ((BufrStringElement)result.Messages[3][0]).Value);
        Assert.Equal(0.43, ((BufrNumberElement)result.Messages[0][1]).Value, 0.001);
        Assert.Equal(0.46, ((BufrNumberElement)result.Messages[3][1]).Value, 0.001);
        Assert.Equal(double.NaN, ((BufrNumberElement)result.Messages[2][1]).Value);
        Assert.Equal(42, ((BufrNumberElement)result.Messages[1][1]).Element.Offset);
        Assert.Equal("Po: 0.44f", ((BufrNumberElement)result.Messages[1][1]).ToString());
        Assert.Equal("Km", ((BufrStringElement)result.Messages[1][0]).Element.Units);
        Assert.Equal("Dm: \"b\"", ((BufrStringElement)result.Messages[1][0]).ToString());
    }

    #endregion

    #region ReadOperation

    [Fact]
    public void ReadOperation_SignifyCharacterOperation_NonZeroLength()
    {
        var token = new Token(2, 5, 6);
        var state = new OperationElementState();
        var obr = new OlieBitReader(Encoding.ASCII.GetBytes("Dillon"));

        var result = Subsets.ReadOperation(token, state, obr);

        Assert.Equal("Dillon", result.ToString());
    }

    [Fact]
    public void ReadOperation_DataWidthOperation_NonZero()
    {
        var token = new Token(2, 1, 130);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        var result = Subsets.ReadOperation(token, state, obr);

        Assert.Equal("Change Data Width by 2", result.ToString());
    }

    [Fact]
    public void ReadOperation_DataWidthOperation_Zero()
    {
        var token = new Token(2, 1, 0);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        var result = Subsets.ReadOperation(token, state, obr);

        Assert.Equal("Change Data Width by 0", result.ToString());
    }

    [Fact]
    public void ReadOperation_ScaleOperation_NonZero()
    {
        var token = new Token(2, 2, 130);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        var result = Subsets.ReadOperation(token, state, obr);

        Assert.Equal("Change Scale by 2", result.ToString());
    }

    [Fact]
    public void ReadOperation_ScaleOperation_Zero()
    {
        var token = new Token(2, 2, 0);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        var result = Subsets.ReadOperation(token, state, obr);

        Assert.Equal("Change Scale by 0", result.ToString());
    }

    [Fact]
    public void ReadOperation_ThrowsNotSupportedException_InvalidToken()
    {
        var token = new Token(2, 10, 0);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        Assert.Throws<NotSupportedException>(() => Subsets.ReadOperation(token, state, obr));
    }

    #endregion

    #region ReadElement

    [Fact]
    public void ReadElement_ThrowsNotSupportedException_UnknownToken()
    {
        var token = new Token(0, 1, 2);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        Assert.Throws<NotSupportedException>(() => Subsets.ReadElement(token, [], state, obr));
    }

    [Fact]
    public void ReadElement_ThrowsInvalidOperationException_InvalidElement()
    {
        var token = new Token(0, 1, 2);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-002", new Element() { Type = (ElementTypesEnum)(-1) } }
        };

        Assert.Throws<InvalidOperationException>(() => Subsets.ReadElement(token, elements, state, obr));
    }

    #endregion

    #region ReadMessage

    [Fact]
    public void ReadMessage_ThrowsNotSupportedException_InvalidToken()
    {
        var token = new Token(3, 1, 2);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        Assert.Throws<NotSupportedException>(() => Subsets.ReadMessage(token, [], state, obr));
    }

    [Fact]
    public void ReadMessage_ReadsOperation_OperationToken()
    {
        var token = new Token(2, 2, 130);
        var state = new OperationElementState();
        var obr = new OlieBitReader([]);

        var result = Subsets.ReadMessage(token, [], state, obr);

        Assert.IsType<ChangeScaleOperation>(result);
        Assert.Equal(2, state.Scale);
    }

    [Fact]
    public void ReadMessage_ReadsReplication_OperationToken()
    {
        var token = new ReplicationToken(new Token(1, 2, 4))
        {
            Tokens = [new(0, 1, 1), new(0, 1, 2)]
        };
        var state = new OperationElementState();
        var data = new List<byte> { (byte)'a', 1, (byte)'b', 2, (byte)'c', 3, (byte)'d', 4 }.ToArray();
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-001", new Element() { Type = ElementTypesEnum.String, Width = 8 } },
            { "0-01-002", new Element() { Type = ElementTypesEnum.Table, Width = 8 } }
        };

        var result = Subsets.ReadMessage(token, elements, state, obr);

        Assert.IsType<BufrReplication>(result);
    }

    #endregion

    #region ReadSubsets

    [Fact]
    public void ReadSubsets_ReturnsSubsets_ValidMessage()
    {
        var subsets = 2;
        var tokens = new List<Token> { new(0, 0, 1), new(0, 0, 2) };
        var elements = new Dictionary<string, Element>
        {
            { "0-00-001", new Element { Type = ElementTypesEnum.String, Width = 8 } },
            { "0-00-002", new Element { Type = ElementTypesEnum.Table, Width = 8 } }
        };
        var data = new List<byte> { (byte)'a', 2, (byte)'b', 4 }.ToArray();
        var obr = new OlieBitReader(data);

        var result = Subsets.ReadSubsets(subsets, tokens, elements, obr);

        Assert.Equal(2, result.Count);
    }

    #endregion
}