using Network.HttpMessages;
using Network.Routing;

namespace Network.Handlers;

public interface IHandler
{
    public Task<Response> Handle(RequestContext requestContext);
}
