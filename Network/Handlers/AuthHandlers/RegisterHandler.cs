using Network.Auth;
using Network.HttpMessages;
using Network.Middlewares;
using System.Text;
using System.Text.Json;

namespace Network.Handlers.AuthHandlers;

public class RegisterHandler : IHandler
{
    private AuthDatabase _database;

    public RegisterHandler(AuthDatabase database)
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

            bool success = _database.Register(payload.Username, payload.Password);

            if (!success)
            {
                return new Response.Builder()
                    .SetStatusCode(400)
                    .Build();
            }

            return new Response.Builder()
                .SetStatusCode(201)
                .SetHeaderContentType("application/json")
                .SetBody(Encoding.UTF8.GetBytes(@"{""message"": ""Création d'un utilisateur avec succès.""}"))
                .Build();
        };
    }

    public class RequestPayload(string username, string password)
    {
        public string Username { get; set; } = username;
        public string Password { get; set; } = password;
    }
}
