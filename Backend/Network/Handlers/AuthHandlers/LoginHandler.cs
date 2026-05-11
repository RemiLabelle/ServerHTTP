using Network.Auth;
using Network.HttpMessages;
using Network.Middlewares;
using System.Text;
using System.Text.Json;

namespace Network.Handlers.AuthHandlers;

/// <summary>
/// Handles authentication requests for existing users and returns a session token cookie on success.
/// </summary>
public class LoginHandler : IHandler
{
    private AuthDatabase _database;

    public LoginHandler(AuthDatabase database)
    {
        _database = database;
    }

    public IHandler.Handler GetHandler()
    {
        return async (requestContext) =>
        {
            RequestPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<RequestPayload>(requestContext.Request.Body, options: new()
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                return new Response.Builder()
                    .SetStatusCode(400)
                    .Build();
            }

            if (payload == null
             || payload.Username == null
             || payload.Password == null)
            {
                return new Response.Builder()
                    .SetStatusCode(400)
                    .Build();
            }

            Token? token = _database.Authenticate(payload.Username, payload.Password);

            if (token == null)
            {
                return new Response.Builder()
                    .SetStatusCode(401)
                    .Build();
            }

            return new Response.Builder()
                .SetStatusCode(200)
                .AddHeader("Set-Cookie", $"{AuthMiddleware.COOKIE_KEY_NAME}={token.value}; HttpOnly; Path=/; SameSite=Lax")
                .SetBody(Encoding.UTF8.GetBytes(@"{""message"": ""Vous êtes connecté avec succès""}"))
                .Build();
        };
    }

    public class RequestPayload(string username, string password)
    {
        public string Username { get; set; } = username;
        public string Password { get; set; } = password;
    }
}
