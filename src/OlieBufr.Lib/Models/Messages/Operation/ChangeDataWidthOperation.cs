namespace OlieBufr.Lib.Models.Messages.Operation;

public class ChangeDataWidthOperation(int y) : IBufrMessage
{
    public int Offset { get; } = y == 0 ? 0 : y - 128;

    public override string ToString() => $"Change Data Width by {Offset}";
}
