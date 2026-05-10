using Network.Handlers;
using static Network.Handlers.IHandler;

namespace Network.Routing.Routes;

public class ServeStaticFileRoute : Route
{
    public ServeStaticFileRoute(string relativePath)
        : base(CreateHandler(relativePath)) { }

    public static Handler CreateHandler(string relativePath)
    {
        ServeStaticFileHandler serveStaticFileHandler = new(Path.GetFullPath(relativePath));

        Handler handler = serveStaticFileHandler.Handle;
        return handler;
    }
}
