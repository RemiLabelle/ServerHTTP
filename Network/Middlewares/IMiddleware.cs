using Network.HttpMessages;
using Network.Routing;
using static Network.Handlers.IHandler;

namespace Network.Middlewares;

public interface IMiddleware
{
    public delegate Task<Response> Middleware(RequestContext requestContext, Handler next);

    public Task<Response> Handle(RequestContext requestContext, Handler next);
}
