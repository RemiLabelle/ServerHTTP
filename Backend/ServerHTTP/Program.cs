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
        apiRouter.AddPost("/publications/", createPublicationRoute);

        apiRouter.AddGet("/publications/", new GetPublicationsHandler(publicationDatabase));

        apiRouter.AddGet("/test/", async (req) =>
        {
            return new Response.Builder()
                .SetStatusCode(200)
                .SetBody(Encoding.UTF8.GetBytes(@"{""Testing"": ""This is working!""}"))
                .Build();
        });

        apiRouter.AddGet("/*", new ServeStaticFileHandler(Path.GetFullPath("./public")));

        var baseRouter = new Router();
        baseRouter.AddGet("/api/*", apiRouter);
        baseRouter.AddPost("/api/*", apiRouter);

        var server = new ServerHTTP(IPAddress.Any, 8808, baseRouter);
        server.OnLogging += (s) => Console.WriteLine(s);
        Console.WriteLine("Appuyez sur une touche pour arrêter le serveur.");
        _ = server.ListenAsync();

        Console.ReadKey();
        Console.WriteLine("Arrêt du serveur");
        server.Stop();
    }
}
