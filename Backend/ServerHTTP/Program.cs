using Network.Auth;
using Network.Handlers;
using Network.Handlers.AuthHandlers;
using Network.Handlers.PublicationHandlers;
using Network.HttpMessages;
using Network.Middlewares;
using Network.Routing;
using Publication;
using System.Net;
using System.Text;

namespace ServerHTTP;

internal class Program
{
    static void Main(string[] args)
    {
        var authDatabase = new AuthDatabase();
        var publicationDatabase = new PublicationDatabase();

        var apiRouter = new Router();

        apiRouter.Middlewares.AddMiddleware(new CompressionMiddleware());

        apiRouter.AddPost("/login/", new LoginHandler(authDatabase));
        apiRouter.AddPost("/register/", new RegisterHandler(authDatabase));

        var authMiddleware = new AuthMiddleware(authDatabase);

        var createPublicationRoute = new Route(new CreatePublicationHandler(publicationDatabase));
        createPublicationRoute.Middlewares.AddMiddleware(authMiddleware);
        apiRouter.AddPost("/publications/", createPublicationRoute);

        var getPublicationsRoute = new Route(new GetPublicationsHandler(publicationDatabase));
        getPublicationsRoute.Middlewares.AddMiddleware(authMiddleware);
        apiRouter.AddGet("/publications/", getPublicationsRoute);

        var baseRouter = new Router();

        baseRouter.Middlewares.AddMiddleware(new CorsMiddleware());

        baseRouter.AddRoute("OPTIONS", "/api/*", apiRouter);

        baseRouter.AddGet("/api/*", apiRouter);
        baseRouter.AddPost("/api/*", apiRouter);

        baseRouter.AddGet("/*", new ServeStaticFileHandler(Path.GetFullPath("../../../../../public")));

        var server = new ServerHTTP(IPAddress.Any, 8808, baseRouter);
        server.OnLogging += (s) => Console.WriteLine(s);
        Console.WriteLine("Appuyez sur une touche pour arrêter le serveur.");
        _ = server.ListenAsync();

        Console.ReadKey();
        Console.WriteLine("Arrêt du serveur");
        server.Stop();
    }
}
