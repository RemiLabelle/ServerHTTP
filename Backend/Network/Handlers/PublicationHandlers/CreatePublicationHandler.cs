using Network.Auth;
using Network.HttpMessages;
using Publication;
using System.Text;
using System.Text.Json;

namespace Network.Handlers.PublicationHandlers;

public class CreatePublicationHandler : IHandler
{
    private PublicationDatabase _database;

    public CreatePublicationHandler(PublicationDatabase database)
    {
        _database = database;
    }

    public IHandler.Handler GetHandler()
    {
        return async (requestContext) =>
        {
            if (!requestContext.Metadata.TryGetValue("user", out var userObj))
                return new Response.Builder()
                    .SetStatusCode(401)
                    .SetHeaderContentType("application/json")
                    .SetBody(Encoding.UTF8.GetBytes(@"{""response"": ""Vous avez besoin d'être logged in. ""}"))
                    .Build();

            User user = (User)userObj;

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
             || payload.Title == null
             || payload.Content == null)
            {
                return new Response.Builder()
                    .SetStatusCode(400)
                    .Build();
            }

            Publication.Publication publication = _database.IncrementOrAdd(user.Name, payload.Title, payload.Content);

            string jsonResponse = JsonSerializer.Serialize(publication, options: new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
            return new Response.Builder()
                .SetStatusCode(201)
                .SetHeaderContentType("application/json")
                .SetBody(Encoding.UTF8.GetBytes(jsonResponse))
                .Build();
        };
    }

    public class RequestPayload(string title, string content)
    {
        public string Title { get; set; } = title;
        public string Content { get; set; } = content;
    }
}
