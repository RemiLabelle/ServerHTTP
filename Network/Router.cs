namespace Network;

public class Router
{
    public delegate Task<Response> Handler(Request request);
    public delegate Task<Response> Middleware(Request request, Handler next);

    private Dictionary<string, Dictionary<string, Route>> _routes = new(StringComparer.OrdinalIgnoreCase);
    private List<Middleware> _middlewares = new();

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

    public void AddMiddleware(Middleware middleware)
    {
        _middlewares.Add(middleware);
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


        Handler pipeline = route.GetHandler();

        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            Middleware currentMiddleware = _middlewares[i];
            Handler next = pipeline;

            pipeline = (request) => currentMiddleware(request, next);
        }

        return await pipeline(request);
    }
}
