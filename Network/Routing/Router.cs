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

    public async Task<Response> HandleRequest(Request request)
    {
        if (!_routes.TryGetValue(request.Path, out var methods))
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();
        if (!methods.TryGetValue(request.Method, out Route? route))
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();


        Handler pipeline = Middlewares.BuildHandler(route.GetHandler());

        return await pipeline(request);
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
