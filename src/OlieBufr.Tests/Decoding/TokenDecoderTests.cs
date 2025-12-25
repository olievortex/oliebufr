using OlieBufr.Lib;
using OlieBufr.Lib.Decoding;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Models.Tokens;

namespace OlieBufr.Tests.Decoding;

public class TokenDecoderTests
{
    #region ReadToken

    [Fact]
    public void ReadToken_ThrowsEndOfStream_WhenNotEnoughBytes()
    {
        var data = new byte[] { 0x01 }; // only one byte, not enough for a descriptor (needs 2)
        using var br = new BinaryReader(new MemoryStream(data));

        Assert.Throws<EndOfStreamException>(() => TokenDecoder.ReadToken(br));
    }

    #endregion

    #region FromDescriptors

    [Fact]
    public void FromDescriptors_ReturnsSingleToken_ForSimpleDescriptor()
    {
        // F=0, X=1, Y=2 -> first byte = (0<<6)|1 = 1, second = 2
        var data = new byte[] { 0x01, 0x02 };

        var result = TokenDecoder.FromDescriptors([], data);

        Assert.Single(result);

        var token = result[0];
        Assert.Equal(0, token.F);
        Assert.Equal(1, token.X);
        Assert.Equal(2, token.Y);
    }

    #endregion

    #region DecodeTokens

    [Fact]
    public void DecodeTokens_ImmediateReplication_ReturnsReplicationToken()
    {
        // Replication descriptor: F=1, X=1, Y=1 -> first byte = (1<<6)|1 = 65, second = 1
        // Inner token: F=0, X=2, Y=3 -> bytes 2,3
        var data = new byte[] { 0x41, 0x01, 0x02, 0x03 };

        var result = TokenDecoder.FromDescriptors([], data);

        Assert.Single(result);
        Assert.IsType<ReplicationToken>(result[0]);

        var rep = (ReplicationToken)result[0];
        Assert.Null(rep.DelayedToken);

        Assert.Single(rep.Tokens);
        var inner = rep.Tokens.First();
        Assert.Equal(0, inner.F);
        Assert.Equal(2, inner.X);
        Assert.Equal(3, inner.Y);
    }

    [Fact]
    public void DecodeTokens_DelayedReplication_ReturnsReplicationToken_WithDelayedToken()
    {
        // Replication descriptor (delayed): F=1, X=1, Y=0 -> first byte = (1<<6)|1 = 65, second = 0
        // Delayed length token must be one of "0-31-001" or "0-31-002" -> use F=0,X=31,Y=1 -> bytes 31,1
        // Work token: F=0, X=2, Y=3 -> bytes 2,3
        var data = new byte[] { 0x41, 0x00, 0x1F, 0x01, 0x02, 0x03 };

        var result = TokenDecoder.FromDescriptors([], data);

        Assert.Single(result);
        Assert.IsType<ReplicationToken>(result[0]);

        var rep = (ReplicationToken)result[0];
        Assert.NotNull(rep.DelayedToken);
        Assert.Equal(0, rep.DelayedToken!.F);
        Assert.Equal(31, rep.DelayedToken.X);
        Assert.Equal(1, rep.DelayedToken.Y);

        Assert.Single(rep.Tokens);
        var inner = rep.Tokens.First();
        Assert.Equal(0, inner.F);
        Assert.Equal(2, inner.X);
        Assert.Equal(3, inner.Y);
    }

    [Fact]
    public void DecodeTokens_OperationToken_ReturnsToken()
    {
        var data = Sequences.TokenToBytes("2-02-124");
        var br = new BinaryReader(new MemoryStream(data));

        var result = TokenDecoder.DecodeTokens([], br);

        Assert.Single(result);
        Assert.Equal("2-02-124", result[0]);
    }

    #endregion

    #region DecodeReplicationToken

    [Fact]
    public void DecodeReplicationToken_NotSupportedException_UnsupportedReplicationFactor()
    {
        var token = new Token(1, 2, 0); // Replicate 2 elements, delayed
        var data = Sequences.TokenToBytes("0-31-003");
        var br = new BinaryReader(new MemoryStream(data));

        Assert.Throws<NotSupportedException>(() => TokenDecoder.DecodeReplicationToken(token, [], br));
    }

    #endregion

    #region DecodeSequenceToken

    [Fact]
    public void DecodeSequenceToken_FlattensTokens_ValidSequence()
    {
        var sequenceTokens = new List<string> { "0-01-001", "0-01-002" };
        var sequenceTokenBytes = sequenceTokens.Select(Sequences.TokenToBytes).SelectMany(m => m).ToArray();
        var sequences = new Dictionary<string, Sequence>
        {
            { "3-01-001", new Sequence() { Bytes = sequenceTokenBytes } }
        };
        var token = Sequences.TokenToBytes("3-01-001");

        var result = TokenDecoder.FromDescriptors(sequences, token);

        Assert.Equal(2, result.Count);
        Assert.Equal("0-01-001", result[0]);
        Assert.Equal("0-01-002", result[1]);
    }

    [Fact]
    public void DecodeSequenceToken_NotSupportedException_MissingSequence()
    {
        var token = Sequences.TokenToBytes("3-01-001");

        Assert.Throws<NotSupportedException>(() => TokenDecoder.FromDescriptors([], token));
    }

    #endregion
}
