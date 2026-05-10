using Network.Handlers;
using Network.HttpMessages;
using static Network.Handlers.IHandler;

namespace Network.Routing;

public class Route : IHandler
{
    private Handler _handler;
    
    public Route(Handler handler)
    {
        _handler = handler;
    }

    public Route(IHandler handler)
    {
        _handler = handler.Handle;
    }

    public Task<Response> Handle(RequestContext requestContext)
    {
        return Middlewares.BuildHandler(_handler)(requestContext);
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
