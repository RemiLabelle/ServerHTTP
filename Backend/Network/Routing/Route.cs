using Network.Handlers;
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

    public Handler GetHandler()
    {
        return Middlewares.BuildHandler(_handler);
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
