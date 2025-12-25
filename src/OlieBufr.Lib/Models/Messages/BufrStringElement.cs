namespace OlieBufr.Lib.Models.Messages;

public class BufrStringElement : IBufrMessage
{
    public Element Element { get; set; } = new();
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Element.Name}: \"{Value}\"";
    }
}
