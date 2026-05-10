using Network.Handlers;
using Network.HttpMessages;
using Network.Middlewares;
using Network.Routing;
using Network.Routing.Routes;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using static Network.NetworkUtils;

namespace ServerHTTP;

internal class Program
{
    static async Task Main(string[] args)
    {
        byte[] testBody = Encoding.UTF8.GetBytes("This is a test body");
        byte[] compressed = await HandleCompressionAsync(
            testBody,
            (outputStream, direction) => new BrotliStream(outputStream, direction),
            CompressionMode.Compress
        );
        //simulation de body encode en Brotli et chunked
        byte[] chunked = await ChunkBodyAsync(compressed, 2);

        var router = new Router();

        //ajoute le middleware au router
        router.Middlewares.AddMiddleware(new CompressionMiddleware());

        router.AddGet("/api/.*", async (requestCtx) => await Task.Run(() =>
        {
            Console.WriteLine("RECEIVED BODY: \n" + Encoding.UTF8.GetString(requestCtx.Request.Body) + "\n____________________\n");

            return new Response.Builder()
                .SetStatusCode(200)
                .SetBody(requestCtx.Request.Body)
                .Build();
        }));

        //simule request chunked et encodee Brotli pour tester le middleware de compression
        var compRes = await router.GetHandler()(
            new RequestContext.Builder()
            .SetRequest(
                new Request.Builder()
                .SetMethod("GET")
                .SetPath("/api/test")
                .AddHeader("Content-Encoding", "br")
                .AddHeader("Transfer-Encoding", "chunked")
                .AddHeader("Accept-Encoding", "br")
                .SetBody(chunked)
                .Build()
            )
            .Build()
        );

        //response envoyee selon le middleware de compression
        Console.WriteLine("SENT: \n" + Encoding.UTF8.GetString(compRes.GetMessage()) + "\n____________________\n");

        //response une fois decompressee par le client
        Console.WriteLine("FIXED BODY: \n" + Encoding.UTF8.GetString(
            await HandleCompressionAsync(
                await DechunkBodyAsync(compRes.Body), (outputStream, direction) => new BrotliStream(outputStream, direction), CompressionMode.Decompress)
            )
        );
    }
}