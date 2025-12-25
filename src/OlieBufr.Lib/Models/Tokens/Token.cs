namespace OlieBufr.Lib.Models.Tokens;

public class Token(int f, int x, int y)
{
    public int F { get; } = f;
    public int X { get; } = x;
    public int Y { get; } = y;

    public override string ToString() => $"{F}-{X:D2}-{Y:D3}";

    public static implicit operator string(Token token) => token.ToString();
}
