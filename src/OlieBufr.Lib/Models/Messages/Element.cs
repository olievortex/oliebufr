namespace OlieBufr.Lib.Models.Messages;

public class Element
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ElementTypesEnum Type { get; set; }
    public int Scale { get; set; }
    public int Offset { get; set; }
    public int Width { get; set; }
    public string Units { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
