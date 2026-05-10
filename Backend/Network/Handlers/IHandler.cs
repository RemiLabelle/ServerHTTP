using Network.HttpMessages;
using Network.Routing;

namespace Network.Handlers;

public interface IHandler
{
    public delegate Task<Response> Handler(RequestContext requestContext);

    public Task<Response> Handle(RequestContext requestContext);

    public static IHandler From(Handler handler) => new SimpleHandler(handler);

    public class SimpleHandler(Handler handler) : IHandler
    {
        public Task<Response> Handle(RequestContext requestContext)
            => handler(requestContext);
    }
}
