using static Network.Router;

namespace Network;

public class Route
{
    private List<Middleware> _middlewares = new();
    private Handler _handler;
    
    public Route(Handler handler)
    {
        _handler = handler;
    }

    public void AddMiddleware(Middleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public Handler GetHandler()
    {
        Handler pipeline = _handler;

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            Middleware currentMiddleware = _middlewares[i];
            Handler next = pipeline;
            pipeline = (request) => currentMiddleware(request, next);
        }

        return pipeline;
    }
}
