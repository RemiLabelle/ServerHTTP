using Network.HttpMessages;
using Network.Routing;

namespace Network.Handlers;

public interface IHandler
{
    public delegate Task<Response> Handler(RequestContext requestContext);

    public async sealed Task<Response> Handle(RequestContext requestContext)
    {
        return await GetHandler()(requestContext);
    }

    public Handler GetHandler();

    public static IHandler From(Handler handler) => new SimpleHandler(handler);

    public class SimpleHandler(Handler handler) : IHandler
    {
        public Handler GetHandler() => handler;
    }
}
