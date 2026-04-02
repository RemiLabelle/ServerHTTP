using Network.HttpMessages;

namespace Network.Handlers;

public interface IHandler
{
    public Task<Response> Handle(Request request);
}
