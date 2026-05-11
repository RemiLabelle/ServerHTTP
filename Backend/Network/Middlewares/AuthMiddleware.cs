using Network.Auth;
using Network.HttpMessages;
using Network.Routing;
using System.Net;
using System.Reflection.PortableExecutable;
using static Network.Handlers.IHandler;

namespace Network.Middlewares;

public class AuthMiddleware : IMiddleware
{
    public const string COOKIE_KEY_NAME = "token";

    private AuthDatabase _database;

    public AuthMiddleware(AuthDatabase database)
    {
        _database = database;
    }

    public Task<Response> Handle(RequestContext requestContext, Handler next)
    {
        Dictionary<string, string> cookies = requestContext.Request.Cookies();

        if (!cookies.TryGetValue(COOKIE_KEY_NAME, out string? tokenString))
            return Task.FromResult(
                new Response.Builder()
                .SetStatusCode(401)
                .Build()
            );

        Token token = new(tokenString);

        User? user = _database.GetUserFromToken(token);

        if (user == null)
            return Task.FromResult(
                new Response.Builder()
                .SetStatusCode(401)
                .Build()
            );

        RequestContext newRequestContext = new RequestContext.Builder(requestContext)
            .AddToMetadata("user", user)
            .Build();

        return next(newRequestContext);
    }
}
