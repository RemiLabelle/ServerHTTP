using System.Security.Cryptography;
using System.Text;

namespace Network.Auth;

public record Token(string value)
{
    public static Token GenerateRandomToken()
    {
        return new Token(Convert.ToBase64String(SHA256.HashData(RandomNumberGenerator.GetBytes(32))));
    }
}
