using Network.HttpMessages;
using Network.Routing;
using static Network.Routing.MiddlewarePipeline;

namespace Network.Middlewares;

public interface IMiddleware
{
    public Task<Response> Handle(RequestContext requestContext, Handler next);
}
