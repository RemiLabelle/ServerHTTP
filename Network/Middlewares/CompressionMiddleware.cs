using Network.HttpMessages;
using Network.Routing;
using System.IO.Compression;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private delegate Stream CompressionStream(Stream output, CompressionMode direction);

        private static readonly Dictionary<string, CompressionStream> _streamGenerator = new()
        {
            { "gzip", (compressor, direction) => new GZipStream(compressor, direction) },
            { "br", (compressor, direction) => new BrotliStream(compressor, direction) }
        };

        private static async Task<byte[]> HandleCompression(byte[] body, CompressionStream streamBuilder, CompressionMode direction) 
        {
            using MemoryStream output = new();

            await using Stream compressor = streamBuilder(output, direction);
            await compressor.WriteAsync(body);

            return output.ToArray();
        }

        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            //decompress as request


            Response response = await next(requestContext);

            //compress as response
            if (requestContext.Request.Headers.TryGetValue("Content-Encoding", out string? encoding))
            {
                byte[] compressedBody = []; // = TODO - await _dispatcher[encoding](requestContext.Request.Body, CompressionMode.Compress)

                new RequestContext.Builder(requestContext)
                .SetRequest(
                    new Request.Builder(requestContext.Request)
                    .SetBody(compressedBody)
                    .Build()
                )
                .Build();
            }

            return response;
        }
    }
}
