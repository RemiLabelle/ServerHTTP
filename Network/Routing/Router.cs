using Network.HttpMessages;
using System.Text.RegularExpressions;
using static Network.Routing.MiddlewarePipeline;

namespace Network.Routing;

public class Router
{
    private List<(Regex, Dictionary<string, Route>)> _routes = new();

    public void AddRoute(string method, Regex path, Route route)
    {
        string prefix = Regex.Replace(path.ToString(), @"[.*+?^${}()|[\]\\]", "");
        if (prefix[prefix.Length - 1] != '/')
            throw new ArgumentException("Path must end with a '/' character", nameof(path));
        Dictionary<string, Route> methods = new(StringComparer.OrdinalIgnoreCase);
        _routes.Add((path, methods));

        methods.Add(method, route);
    }

    public void AddRoute(string method, string path, Route route)
    {
        AddRoute(method, new Regex("^" + path), route);
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
        Dictionary<string, Route>? methods = null;
        string? prefix = null;
        foreach ((Regex pathPattern, Dictionary<string, Route> currentMethods) in _routes)
        {
            Match match = pathPattern.Match(requestContext.Request.Path);
            if (match.Success)
            {
                methods = currentMethods;
                prefix = Regex.Replace(pathPattern.ToString(), @"[.*+?^${}()|[\]\\]", "");
                break;
            }
        }

        if (methods == null
            || prefix == null)
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();

        if (!methods.TryGetValue(requestContext.Request.Method, out Route? route))
            return new Response.Builder()
                .SetStatusCode(404)
                .Build();

        RequestContext newRequestContext = new RequestContext.Builder(requestContext)
            .AppendToPrefix(prefix)
            .Build();

        Handler pipeline = Middlewares.BuildHandler(route.GetHandler());

        try
        {
            return await pipeline(newRequestContext);
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
