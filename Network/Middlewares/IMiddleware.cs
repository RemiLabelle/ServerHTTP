using Network.HttpMessages;
using static Network.Routing.MiddlewarePipeline;

namespace Network.Middlewares;

public interface IMiddleware
{
    public Task<Response> Handle(Request request, Handler next);
}
