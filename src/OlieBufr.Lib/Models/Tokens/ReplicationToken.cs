namespace OlieBufr.Lib.Models.Tokens;

public class ReplicationToken(Token token) : Token(token.F, token.X, token.Y)
{
    public List<Token> Tokens { get; set; } = [];
    public Token? DelayedToken { get; set; }
}
