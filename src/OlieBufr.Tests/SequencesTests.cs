using Moq;
using OlieBufr.Lib;
using OlieBufr.Lib.Services;
using System.Text;

namespace OlieBufr.Tests;

public class SequencesTests
{
    private static IOlieService GetIOlieService()
    {
        var csv = new StringBuilder()
            .AppendLine("[{\"Id\":\"3-00-003\",\"Name\":\"FXYDESCR\",\"Description\":\"F, X, Y of descriptor to be added or defined\",\"Elements\":[ \"0-00-010\",\"0-00-011\",\"0-00-012\"]},")
            .AppendLine("{\"Id\":\"3-00-004\",\"Name\":\"ELUNSCRW\",\"Description\":\"Element unscaled raw data\",\"Elements\":[\"3-00-003\",\"0-00-013\",\"0-00-014\",\"0-00-015\",\"0-00-016\",\"0-00-017\",\"0-00-018\",\"0-00-019\",\"0-00-020\"]},")
            .AppendLine("{\"Id\":\"3-01-001\",\"Name\":\"WMOBLKST\",\"Description\":\"WMO block and station numbers\",\"Elements\":[\"0-01-001\",\"0-01-002\"]}]")
            .ToString();

        var olieService = new Mock<IOlieService>();
        olieService.Setup(s => s.ReadSequencesJson())
            .Returns(csv);

        return olieService.Object;
    }

    [Fact]
    public void Reference_LoadsSequences_FromJson()
    {
        Sequences.ClearCache();

        var sequences = new Sequences(GetIOlieService());
        var reference = sequences.Reference;

        Assert.NotNull(reference);
        Assert.Equal(3, reference.Count);
        Assert.True(reference.ContainsKey("3-00-003"));
        Assert.True(reference.ContainsKey("3-00-004"));
        Assert.True(reference.ContainsKey("3-01-001"));
        Assert.NotNull(reference["3-00-003"]);
        Assert.NotNull(reference["3-00-004"]);
        Assert.NotNull(reference["3-01-001"]);
        Assert.Equal("3-00-003 - FXYDESCR", reference["3-00-003"].ToString());
        Assert.Equal("F, X, Y of descriptor to be added or defined", reference["3-00-003"].Description);
    }

    [Fact]
    public void Reference_IsCached_AcrossInstances()
    {
        Sequences.ClearCache();

        var sequences1 = new Sequences(GetIOlieService());
        var ref1 = sequences1.Reference;

        // second service returns different CSV, but because _reference is static it should not be reloaded
        var sequences2 = new Sequences(GetIOlieService());
        var ref2 = sequences2.Reference;

        Assert.Same(ref1, ref2);
        Assert.True(ref1.ContainsKey("3-00-003"));
    }

    [Fact]
    public void Reference_Empty_NullJson()
    {
        Sequences.ClearCache();

        var olieService = new Mock<IOlieService>();
        olieService.Setup(s => s.ReadSequencesJson())
            .Returns("null");

        var sequences = new Sequences(olieService.Object);
        var reference = sequences.Reference;

        Assert.NotNull(reference);
        Assert.Empty(reference);
    }
}
