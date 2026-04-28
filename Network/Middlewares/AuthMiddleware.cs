using Network.Auth;
using Network.HttpMessages;
using Network.Routing;
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
        if (!cookies.TryGetValue(COOKIE_KEY_NAME, out string? token))
            return Task.FromResult(
                new Response.Builder()
                .SetStatusCode(401)
                .Build()
            );
        return next(requestContext);
    }
}
