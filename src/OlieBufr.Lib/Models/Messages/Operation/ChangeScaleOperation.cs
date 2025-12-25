namespace OlieBufr.Lib.Models.Messages.Operation;

public class ChangeScaleOperation(int y) : IBufrMessage
{
    public int Offset { get; } = y == 0 ? 0 : y - 128;

    public override string ToString() => $"Change Scale by {Offset}";
}
