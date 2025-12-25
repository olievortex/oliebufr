using OlieBufr.Lib.Services;

namespace OlieBufr.Lib.Models.Messages.Operation;

/// <summary>
/// 2-05-YYY Signify character string of YYY characters
/// </summary>
/// <param name="y"></param>
/// <param name="data"></param>
public class SignifyCharacterOperation(int y, OlieBitReader data) : IBufrMessage
{
    public string Value { get; } = data.ReadFixedLengthString(y);

    public override string ToString() => Value;
}
