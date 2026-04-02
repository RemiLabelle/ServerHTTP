namespace Network;

public interface IHandler
{
    public Task<Response> Handle(Request request);
}
