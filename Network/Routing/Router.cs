using Network.HttpMessages;
using static Network.Routing.MiddlewarePipeline;

namespace Network.Routing;

public class Router
{
    private Dictionary<string, Dictionary<string, Route>> _routes = new(StringComparer.OrdinalIgnoreCase);

    public void AddRoute(string method, string path, Route route)
    {
        Dictionary<string, Route>? methods;
        if (!_routes.TryGetValue(path, out methods))
        {
            methods = new(StringComparer.OrdinalIgnoreCase);
            _routes.Add(path, methods);
        }

        methods.Add(method, route);
    }

    public void AddRoute(string method, string path, Handler handler)
    {
        AddRoute(method, path, new Route(handler));
    }

    public void AddGet(string path, Route route)
    {
        AddRoute("GET", path, route);
    }

    public void AddGet(string path, Handler handler)
    {
        AddRoute("GET", path, handler);
    }

    public void AddPost(string path, Route route)
    {
        AddRoute("POST", path, route);
    }

    public void AddPost(string path, Handler handler)
    {
        AddRoute("POST", path, handler);
    }

    public async Task<Response> HandleRequest(RequestContext requestContext)
    {
        if (!_routes.TryGetValue(requestContext.Request.Path, out var methods))
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();
        if (!methods.TryGetValue(requestContext.Request.Method, out Route? route))
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();

        Handler pipeline = Middlewares.BuildHandler(route.GetHandler());

        try
        {
            return await pipeline(requestContext);
        }
        catch
        {
            return new Response.Builder()
                .SetStatusCode(500)
                .Build();
        }
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
