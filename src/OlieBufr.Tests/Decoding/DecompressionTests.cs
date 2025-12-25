using OlieBufr.Lib.Decoding;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Tokens;
using OlieBufr.Lib.Services;

namespace OlieBufr.Tests.Decoding;

public class DecompressionTests
{
    #region ReadSubsets

    [Fact]
    public void ReadSubsets_PivotsResults_ValidData()
    {
        var subsets = 2;
        var tokens = new List<Token> { new(0, 1, 1), new(0, 1, 2) };
        var elements = new Dictionary<string, Element>
        {
            { "0-01-001", new Element() { Width = 8, Type = ElementTypesEnum.Table } },
            { "0-01-002", new Element() { Width = 10, Type = ElementTypesEnum.Number } }
        };
        var data = new List<byte> { 0x08, 0b00000000, 0x10, 0b00000101 }.ToArray();
        var obr = new OlieBitReader(data);

        var result = Decompression.ReadSubsets(subsets, tokens, elements, obr);

        Assert.Equal(2, result.Count);
        Assert.Equal(8, ((BufrIntElement)result[0][0]).Value);
        Assert.Equal(16, ((BufrNumberElement)result[0][1]).Value, 0.001);
        Assert.Equal(8, ((BufrIntElement)result[1][0]).Value);
        Assert.Equal(17, ((BufrNumberElement)result[1][1]).Value, 0.001);
    }

    #endregion

    #region DecodeValues

    [Fact]
    public void DecodeValues_OnlyOperationTokens_ReturnsArrayOfNulls()
    {
        // Arrange
        var tokens = new List<Token> {
            new(2, 1, 0), // F=2, X=1 -> ChangeDataWidthOperation
            new(2, 2, 0)  // F=2, X=2 -> ChangeScaleOperation
        };
        var elements = new Dictionary<string, Element>();

        // Act
        var result = Decompression.DecodeValues(1, tokens, elements, null!);

        // Assert
        Assert.Equal(tokens.Count, result.Length);
        foreach (var item in result)
        {
            Assert.Null(item);
        }
    }

    [Fact]
    public void DecodeValues_ReplicationToken_ExpandsTokens()
    {
        // Arrange
        var innerToken = new Token(2, 1, 0); // operation token inside replication
        var replicationToken = new ReplicationToken(new Token(1, 1, 3))
        {
            Tokens = [innerToken]
        };
        var tokens = new List<Token> { replicationToken };
        var elements = new Dictionary<string, Element>();

        // Act
        var result = Decompression.DecodeValues(1, tokens, elements, null!);

        // Assert
        Assert.Equal(3, result.Length); // replication expands to 3 occurrences
        foreach (var item in result)
        {
            Assert.Null(item);
        }
    }

    [Fact]
    public void DecodeValues_ThrowsNotImplementedException_DelayedReplication()
    {
        // Arrange
        var innerToken = new Token(2, 1, 0); // operation token inside replication
        var replicationToken = new ReplicationToken(new Token(1, 1, 0))
        {
            Tokens = [innerToken]
        };
        var tokens = new List<Token> { replicationToken };
        var elements = new Dictionary<string, Element>();

        // Act, Assert
        Assert.Throws<NotImplementedException>(() => Decompression.DecodeValues(1, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_ElementNotFound_ThrowsNotSupportedException()
    {
        // Arrange
        var token = new Token(0, 0, 1); // non-operation token that should map to an element
        var tokens = new List<Token> { token };
        var elements = new Dictionary<string, Element>();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            Decompression.DecodeValues(1, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_TokenF1_ThrowsNotImplementedException()
    {
        // Arrange
        var token = new Token(1, 0, 0); // F=1 path is not implemented
        var tokens = new List<Token> { token };
        var elements = new Dictionary<string, Element>();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            Decompression.DecodeValues(1, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_TokenF2X0_ThrowsNotImplementedException()
    {
        // Arrange
        var token = new Token(2, 0, 0); // F=2 path is not implemented
        var tokens = new List<Token> { token };
        var elements = new Dictionary<string, Element>();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            Decompression.DecodeValues(1, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_ReturnsValues_SimpleIncrementInt()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var data = new byte[] { 16, 0 };
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.Table, Width = 8 } }
        };

        // Act
        var result = Decompression.DecodeValues(10, tokens, elements, obr);

        // Assert
        Assert.Single(result);
        Assert.Equal(10, result[0].Count);
        Assert.Equal(16, ((BufrIntElement)result[0][0]).Value);
    }

    [Fact]
    public void DecodeValues_ReturnsValues_BitIncrementInt()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var data = new byte[] { 16, 0b00000111, 0b11100000 };
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.Table, Width = 8 } }
        };

        // Act
        var result = Decompression.DecodeValues(10, tokens, elements, obr);

        // Assert
        Assert.Single(result);
        Assert.Equal(10, result[0].Count);
        Assert.Equal(17, ((BufrIntElement)result[0][0]).Value);
        Assert.Equal(16, ((BufrIntElement)result[0][9]).Value);
    }

    [Fact]
    public void DecodeValues_ArgumentException_SimpleIncrementIntElementMismatch()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.Table, Scale = 12, Width = 8 } }
        };

        // Act, Assert
        Assert.Throws<InvalidOperationException>(() => Decompression.DecodeValues(10, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_ArgumentException_IncrementString()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.String, Scale = 12, Width = 8 } }
        };

        // Act, Assert
        Assert.Throws<NotImplementedException>(() => Decompression.DecodeValues(10, tokens, elements, null!));
    }

    [Fact]
    public void DecodeValues_ReturnsValues_SimpleIncrementNumber()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var data = new byte[] { 16, 0 };
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.Number, Width = 8, Scale = 2, Offset = 4 } }
        };

        // Act
        var result = Decompression.DecodeValues(10, tokens, elements, obr);

        // Assert
        Assert.Single(result);
        Assert.Equal(10, result[0].Count);
        Assert.Equal(.2, ((BufrNumberElement)result[0][0]).Value, .001);
    }

    [Fact]
    public void DecodeValues_ReturnsValues_BitIncrementNumber()
    {
        // Arrange
        var token = new Token(0, 1, 10);
        var tokens = new List<Token> { token };
        var data = new byte[] { 252, 0b00001000, 0b01101100 };
        var obr = new OlieBitReader(data);
        var elements = new Dictionary<string, Element>
        {
            { "0-01-010", new Element() { Type = ElementTypesEnum.Number, Width = 8, Scale = 2, Offset = 4 } }
        };

        // Act
        var result = Decompression.DecodeValues(4, tokens, elements, obr);

        // Assert
        Assert.Single(result);
        Assert.Equal(4, result[0].Count);
        Assert.Equal(2.56, ((BufrNumberElement)result[0][0]).Value, .001);
        Assert.Equal(2.57, ((BufrNumberElement)result[0][1]).Value, .001);
        Assert.Equal(2.58, ((BufrNumberElement)result[0][2]).Value, .001);
        Assert.Equal(double.NaN, ((BufrNumberElement)result[0][3]).Value);
    }

    #endregion
}
