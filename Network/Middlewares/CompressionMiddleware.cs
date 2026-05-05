using Network.HttpMessages;
using Network.Routing;
using System.IO.Compression;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private static async Task<byte[]> HandleCompression<T>(byte[] body, CompressionMode direction) where T : Stream, new()
        {
            using MemoryStream outputStream = new();
            using T compressor = new();

            await compressor.WriteAsync(body);
            return outputStream.ToArray();
        }

        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            //decompress as request


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

            return await next(requestContext);
        }
    }
}
