using Network.Middlewares;
using static Network.Handlers.IHandler;
using static Network.Middlewares.IMiddleware;

namespace Network.Routing;

/// <summary>
/// Builds a middleware execution chain around a final request handler.
/// </summary>
public class MiddlewarePipeline
{
    private List<Middleware> _middlewares = new();

    public void AddMiddleware(Middleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public void AddMiddleware(IMiddleware middleware)
    {
        _middlewares.Add(middleware.Handle);
    }

    public Handler BuildHandler(Handler final)
    {
        Handler pipeline = final;

        for (int i = _middlewares.Count - 1; i >= 0; --i)
        {
            Middleware middleware = _middlewares[i];
            Handler next = pipeline;

            pipeline = (request) => middleware(request, next);
        }

        return pipeline;
    }
}
