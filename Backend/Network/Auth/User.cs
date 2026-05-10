namespace Network.Auth;

public class User(string name, string password)
{
    public string Name { get; set; } = name;
    public string Password { get; set; } = password;
}
