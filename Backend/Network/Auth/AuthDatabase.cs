namespace Network.Auth;

/// <summary>
/// Stores users and authentication tokens in memory and provides registration and lookup operations.
/// </summary>
public class AuthDatabase
{
    private IList<User> _users = new List<User>();
    private IDictionary<Token, User> _tokens = new Dictionary<Token, User>();

    public Token? Authenticate(string username, string password)
    {
        foreach (var user in _users)
        {
            if (user.Name == username && user.Password == password)
            {
                Token token = Token.GenerateRandomToken();
                _tokens.Add(token, user);
                return token;
            }
        }

        return null;
    }

    public bool Register(string username, string password)
    {
        foreach (var user in _users)
        {
            if (user.Name == username)
                return false;
        }

        _users.Add(new User(username, password));

        return true;
    }

    public User? GetUserFromToken(Token authToken)
    {
        foreach (var token in _tokens)
        {
            if (token.Key.Equals(authToken))
                return token.Value;
        }

        return null;
    }
}
