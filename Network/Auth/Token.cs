using System.Security.Cryptography;

namespace Network.Auth;

public record Token(string value)
{
    public static Token GenerateRandomToken()
    {
        return new Token(SHA256.HashData(RandomNumberGenerator.GetBytes(32)).ToString()!);
    }
}
