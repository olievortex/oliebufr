namespace OlieBufr.Lib.Models.Messages;

public class BufrReplication : IBufrMessage
{
    public List<List<IBufrMessage>> Messages { get; } = [];

    public override string ToString()
    {
        return $"Count = {Messages.Count}";
    }
}
