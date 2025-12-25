using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace OlieBufr.Lib.Services;

[ExcludeFromCodeCoverage]
public class OlieService : IOlieService
{
    public string ReadSequencesJson()
    {
        return File.ReadAllText("sequences.json", Encoding.ASCII);
    }

    public byte[] ReadElementsCsv()
    {
        return File.ReadAllBytes("elements.csv");
    }
}
