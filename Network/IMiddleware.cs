using static Network.MiddlewarePipeline;

namespace Network;

public interface IMiddleware
{
    public Task<Response> Handle(Request request, Handler next);
}
