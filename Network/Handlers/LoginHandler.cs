using Network.Auth;
using Network.HttpMessages;
using Network.Middlewares;
using System.Text.Json;

namespace Network.Handlers;

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
                payload = JsonSerializer.Deserialize<RequestPayload>(requestContext.Request.Body);
            }
            catch (JsonException)
            {
                return new Response.Builder()
                    .SetStatusCode(400)
                    .Build();
            }

            if (payload == null)
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
                .AddHeader("Set-Cookie", $"{AuthMiddleware.COOKIE_KEY_NAME}={token.value}; HttpOnly")
                .Build();
        };
    }

    public class RequestPayload(string username, string password)
    {
        public string Username { get; set; } = username;
        public string Password { get; set; } = password;
    }
}
