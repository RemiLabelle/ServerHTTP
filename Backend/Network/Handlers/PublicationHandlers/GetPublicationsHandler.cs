using Network.HttpMessages;
using Network.Routing;
using Publication;
using System.Text;
using System.Text.Json;

namespace Network.Handlers.PublicationHandlers;

public class GetPublicationsHandler : IHandler
{
    private PublicationDatabase _database;
    
    public GetPublicationsHandler(PublicationDatabase database)
    {
        _database = database;
    }

    public async Task<Response> Handle(RequestContext requestContext)
    {
        Dictionary<string, string> pathParams = requestContext.Request.GetParams();
        List<Publication.Publication> publications;

        if (pathParams.TryGetValue("username", out var username))
        {
            publications = _database.GetPublicationsFromUsername(username) ?? new();
        }
        else
        {
            publications = _database.All();
        }

        string jsonResponse = JsonSerializer.Serialize(publications, options: new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        return new Response.Builder()
            .SetStatusCode(200)
            .SetHeaderContentType("application/json")
            .SetBody(Encoding.UTF8.GetBytes(jsonResponse))
            .Build();
    }
}
