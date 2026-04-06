using Network.Handlers;
using Network.HttpMessages;
using System.Text.RegularExpressions;
using static Network.Handlers.IHandler;

namespace Network.Routing;

public class Router : IHandler
{
    private List<(Regex, Dictionary<string, IHandler>)> _routes = new();

    public void AddRoute(string method, Regex path, IHandler handler)
    {
        string prefix = Regex.Replace(path.ToString(), @"[.*+?^${}()|[\]\\]", "");
        if (prefix[prefix.Length - 1] != '/')
            throw new ArgumentException("Path must end with a '/' character", nameof(path));
        Dictionary<string, IHandler> methods = new(StringComparer.OrdinalIgnoreCase);
        _routes.Add((path, methods));

        methods.Add(method, handler);
    }

    public void AddRoute(string method, string path, IHandler handler)
    {
        AddRoute(method, new Regex("^" + path), handler);
    }

    public void AddRoute(string method, string path, Handler handler)
    {
        AddRoute(method, path, IHandler.From(handler));
    }

    public void AddGet(string path, IHandler handler)
    {
        AddRoute("GET", path, handler);
    }

    public void AddPost(string path, IHandler handler)
    {
        AddRoute("POST", path, handler);
    }

    public void AddGet(string path, Handler handler)
    {
        AddRoute("GET", path, handler);
    }

    public void AddPost(string path, Handler handler)
    {
        AddRoute("POST", path, handler);
    }

    public Handler GetHandler()
    {
        return async (requestContext) =>
        {
            Dictionary<string, IHandler>? methods = null;
            string? prefix = null;
            foreach ((Regex pathPattern, Dictionary<string, IHandler> currentMethods) in _routes)
            {
                string path = requestContext.Request.Path;
                string previousPrefix = requestContext.Prefix;
                Match match = pathPattern.Match(path[(previousPrefix.Length - 1)..]);
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

            if (!methods.TryGetValue(requestContext.Request.Method, out IHandler? route))
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
        };
    }

    public MiddlewarePipeline Middlewares { get; private set; } = new();
}
