using Network.Handlers;
using Network.HttpMessages;
using Network.Middlewares;
using Network.Routing;
using Network.Routing.Routes;
using System.Diagnostics;
using System.Text;

namespace ServerHTTP;

internal class Program
{
    static async Task Main(string[] args)
    {
        string strRequest_01 = "GET / HTTP/1.1\r\nCustom-Header: value of header\r\nOther-Header: hello\r\nContent-Length: 18\r\n\r\n{ \"status\": \"ok\" }";
        Request parsedRequest = Request.Parse(strRequest_01); // TryParse(string input, out Request? result) existe
        string strRequest_02 = Encoding.UTF8.GetString(parsedRequest.GetMessage()); // Get full message avec headers et body en byte[]
        Console.WriteLine($"Asserting {nameof(strRequest_01)} and {nameof(strRequest_02)}.");
        Debug.Assert(string.Equals(strRequest_01, strRequest_02));

        // Pour créer une Request ou Response (~ pareil)
        Response builtResponse = new Response.Builder()
            .SetStatusCode(200)
            .SetHeaderContentType("text/plain") // Plusieurs helper pour les headers (voir class Message.Builder, Request.Builder et Response.Builder)
                                                // Possible d'en rajouté si manque un header important
            .AddHeader("Custom-Header", "value")
            .SetBody(Encoding.UTF8.GetBytes("Hello")) // Set et ajout body en byte[]
            .AddToBody(Encoding.UTF8.GetBytes(" World!"))
            .Build(); // Build

        Console.WriteLine($"La built response:\n-----\n{Encoding.UTF8.GetString(builtResponse.GetMessage())}\n-----");

        // Les Messages (Request et Response) ne sont pas mutable/modifiable
        // Il faut en créer un nouveau si on veux updaté le contenue

        Response modifiedResponse = new Response.Builder(builtResponse) // Peux prendre déjà un message
            .SetBody(Encoding.UTF8.GetBytes("New body, old headers"))
            .Build();

        Console.WriteLine($"Modified response:\n-----\n{Encoding.UTF8.GetString(modifiedResponse.GetMessage())}\n-----");

        // Un router permet d'avoir des routes dans l'application
        Router router = new Router();

        // Le router peux prendre un middleware qui implémente l'interface IMiddleware globalement
        // class SimpleMiddleware : IMiddleware
        // {
        //     public Task<Response> Handle(RequestContext requestContext, MiddlewarePipeline.Handler next)
        //     {
        //         Console.WriteLine("Writing from a simple middleware!");
        //         return next(requestContext);
        //     }
        // }
        router.Middlewares.AddMiddleware(new SimpleMiddleware());

        // On peut aussi rajouté un middleware en lambda
        router.Middlewares.AddMiddleware((requestCtx, next) => // Un middleware ne prend pas une request directement
        {                                                      // mais bien un context qui contient une request avec + d'info
            RequestContext newRequestContext = new RequestContext.Builder() // Voici une façon de changer la request
                .SetRequest(                                                // avant de l'envoyé au prochain maillon.
                    new Request.Builder(requestCtx.Request)                 // La partie du set request et le builder
                    .AddHeader("Beader-By-Middleware", "was added before")  // est un peu laide, donc p-ê a changé
                    .Build()
                )
                .Build();
            return next(requestCtx);
        });
        // Ce middleware change la response après que tous ai roulé incluant le handler final
        router.Middlewares.AddMiddleware(async (requestCtx, next) => // Peut être async
        {
            Response response = await next(requestCtx);
            Response newResponse = new Response.Builder(response)
                .AddHeader("Aeader-By-Middleware", "was added after")
                .Build();
            return newResponse;
        });

        // Ajout des routes, peut être une lambda ou class implémentant IHandler
        router.AddGet("/api/.*", async (requestCtx) => // Premier argument est un REGEX
        {
            Console.WriteLine("Bonjour de la route /api/.*");
            return new Response.Builder().Build(); // default response
        });

        router.AddGet("/static/.*", new ServeStaticFileRoute("./public")); // si dans bin il y a public/ va servir les fichiers

        var res = await router.GetHandler()(
            new RequestContext.Builder()
            .SetRequest(
                new Request.Builder()
                .SetMethod("GET")
                .SetPath("/static/index.html")
                .SetHeaderContentType("plain/text")
                .SetBody(Encoding.UTF8.GetBytes("This is a test body"))
                .Build()
            )
            .Build()
        );

        Console.WriteLine(Encoding.UTF8.GetString(res.GetMessage()));
    }

    class SimpleMiddleware : IMiddleware
    {
        public Task<Response> Handle(RequestContext requestContext, IHandler.Handler next)
        {
            Console.WriteLine("Writing from a simple middleware!");
            return next(requestContext);
        }
    }
}