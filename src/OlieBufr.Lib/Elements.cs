using CsvHelper;
using OlieBufr.Lib.Models.Messages;
using OlieBufr.Lib.Services;

namespace OlieBufr.Lib;

public class Elements(IOlieService olieService)
{
    private static Dictionary<string, Element>? _reference;
    public Dictionary<string, Element> Reference
    {
        get
        {
            _reference ??= GetElements(olieService);

            return _reference;
        }
    }

    private static Dictionary<string, Element> GetElements(IOlieService olieService)
    {
        var csvConfig = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
        };

        using var csvText = new StreamReader(new MemoryStream(olieService.ReadElementsCsv()));
        using var csv = new CsvReader(csvText, csvConfig);

        var elements = csv.GetRecords<Element>();

        return elements.ToDictionary(seq => seq.Id, seq => seq);
    }
}
