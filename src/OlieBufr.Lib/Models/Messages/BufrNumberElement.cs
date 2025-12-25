namespace OlieBufr.Lib.Models.Messages;

public class BufrNumberElement : IBufrMessage
{
    public Element Element { get; set; } = new();
    public double Value { get; set; }

    public override string ToString()
    {
        return $"{Element.Name}: {Value}f";
    }
}
