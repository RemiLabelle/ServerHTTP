using Network.HttpMessages;
using Network.Middlewares;

namespace Network.Routing;

public class MiddlewarePipeline
{
    public delegate Task<Response> Handler(RequestContext requestContext);
    public delegate Task<Response> Middleware(RequestContext requestContext, Handler next);

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
