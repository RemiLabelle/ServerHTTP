using Network.HttpMessages;
using Network.Routing;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private static readonly Dictionary<string, Func<byte[], Task<byte[]>>> _dispatcher = new()
        {
            { "gzip", CompressAsGzipAsync },
            { "br", CompressAsBrotliAsync }
        };

        private static async Task<byte[]> CompressAsGzipAsync(byte[] rawBody)
        {
            using MemoryStream outputStream = new();
            using GZipStream compressor = new(outputStream, CompressionMode.Compress);

            await compressor.WriteAsync(rawBody);
            return outputStream.ToArray();
        }

        private static async Task<byte[]> CompressAsBrotliAsync(byte[] rawBody) 
        {
            using MemoryStream outputStream = new();
            using BrotliStream compressor = new(outputStream, CompressionMode.Compress);

            await compressor.WriteAsync(rawBody);
            return outputStream.ToArray();
        }

        private static async Task<byte[]> DecompressAsGzipAsync()
        {
            return []; //todo
        }

        private static async Task<byte[]> DecompressAsBrotliAsync() 
        {
            return []; //todo
        }

        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            //decompress as request


            //compress as response
            if (requestContext.Request.Headers.TryGetValue("Content-Encoding", out string? encoding))
            {
                byte[] compressedBody = await _dispatcher[encoding](requestContext.Request.Body);

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
