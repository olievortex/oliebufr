namespace OlieBufr.Lib.Models.Messages;

public class Sequence
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Elements { get; set; } = [];
    public byte[] Bytes { get; set; } = [];

    public override string ToString()
    {
        return $"{Id} - {Name}";
    }
}
