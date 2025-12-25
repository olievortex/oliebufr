namespace OlieBufr.Lib.Models.Messages;

public class BufrIntElement : IBufrMessage
{
    public Element Element { get; set; } = new();
    public int Value { get; set; }

    public override string ToString()
    {
        return $"{Element.Name}: {Value}";
    }
}
