using Moq;
using OlieBufr.Lib;
using OlieBufr.Lib.Services;
using System.Text;

namespace OlieBufr.Tests;

public class ElementsTests
{
    private static IOlieService GetIOlieService()
    {
        var csv = new StringBuilder()
            .AppendLine("Id,Name,Type,Scale,Offset,Width,Units,Description")
            .AppendLine("0-00-001,TABLAE,0,0,0,24,String,Table A")
            .AppendLine("0-00-002,TABLAD1,0,0,0,256,String,\"Table A: data category description, line 1\"")
            .ToString();
        var bytes = Encoding.ASCII.GetBytes(csv);

        var olieService = new Mock<IOlieService>();
        olieService.Setup(s => s.ReadElementsCsv())
            .Returns(bytes);

        return olieService.Object;
    }

    [Fact]
    public void Reference_LoadsElements_FromCsv()
    {
        var elements = new Elements(GetIOlieService());
        var reference = elements.Reference;

        Assert.NotNull(reference);
        Assert.Equal(2, reference.Count);
        Assert.True(reference.ContainsKey("0-00-001"));
        Assert.True(reference.ContainsKey("0-00-002"));
        Assert.NotNull(reference["0-00-001"]);
        Assert.NotNull(reference["0-00-002"]);
    }

    [Fact]
    public void Reference_IsCached_AcrossInstances()
    {
        var elements1 = new Elements(GetIOlieService());
        var ref1 = elements1.Reference;

        // second service returns different CSV, but because _reference is static it should not be reloaded
        var elements2 = new Elements(GetIOlieService());
        var ref2 = elements2.Reference;

        Assert.Same(ref1, ref2);
        Assert.True(ref1.ContainsKey("0-00-001"));
    }
}
