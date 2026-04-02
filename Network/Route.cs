using static Network.MiddlewarePipeline;

namespace Network;

public class Route
{
    private Handler _handler;
    
    public Route(Handler handler)
    {
        _handler = handler;
    }

    public Handler GetHandler()
    {
        return Middlewares.BuildHandler(_handler);
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
