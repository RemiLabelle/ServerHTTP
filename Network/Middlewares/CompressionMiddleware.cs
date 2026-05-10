using Network.HttpMessages;
using Network.Routing;
using static Network.NetworkUtils;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private static readonly Dictionary<string, GenerateCompressionStream> _streamGenerator = new()
        {
            { "gzip", (compressor, direction) => new GZipStream(compressor, direction) },
            { "br", (compressor, direction) => new BrotliStream(compressor, direction) },
            { "deflate", (compressor, direction) => new DeflateStream(compressor, direction) },
            { "zlib", (compressor, direction) => new ZLibStream(compressor, direction) }
        };

        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            byte[] body = requestContext.Request.Body;

            //organize chunked request
            if (requestContext.Request.Headers.TryGetValue("Transfer-Encoding", out string? transfer)
                && transfer.Contains("chunked"))
            {
                body = await DechunkBodyAsync(body);
            }

            //decode compressed request
            if (requestContext.Request.Headers.TryGetValue("Content-Encoding", out string? encoding)
                && _streamGenerator.TryGetValue(encoding, out GenerateCompressionStream? streamBuilder))
            {
                try
                {
                    body = await HandleCompressionAsync(body, streamBuilder, CompressionMode.Decompress);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //rebuild request with organized and decompressed body
            requestContext = new RequestContext.Builder(requestContext)
                .SetRequest(
                    new Request.Builder(requestContext.Request)
                    .SetBody(body)
                    .Build()
                )
                .Build();

            //process request
            Response response = await next(requestContext);

            //compress response before forwarding
            if (requestContext.Request.Headers.TryGetValue("Accept-Encoding", out encoding)
                && _streamGenerator.TryGetValue(encoding, out streamBuilder))
            {
                byte[] compressedBody = await HandleCompressionAsync(response.Body, streamBuilder, CompressionMode.Compress);
                byte[] chunkedBody = await ChunkBodyAsync(compressedBody);

                response = new Response.Builder(response)
                    .SetBody(chunkedBody)
                    .AddHeader("Content-Encoding", encoding)
                    .AddHeader("Transfer-Encoding", "chunked")
                    .Build();
            }

            return response;
        }
    }
}
