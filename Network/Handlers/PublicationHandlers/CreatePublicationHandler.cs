using Network.HttpMessages;
using System.Text;

namespace Network.Handlers.PublicationHandlers;

public class CreatePublicationHandler : IHandler
{
    public IHandler.Handler GetHandler()
    {
        return async (requestContext) =>
        {
            if (!requestContext.Metadata.TryGetValue("user", out var user))
                return new Response.Builder()
                    .SetStatusCode(201)
                    .SetHeaderContentType("application/json")
                    .SetBody(Encoding.UTF8.GetBytes(@"{""response"": ""Vous avez besoin d'être logged in. ""}"))
                    .Build();
            throw new NotImplementedException();
        };
    }
}
