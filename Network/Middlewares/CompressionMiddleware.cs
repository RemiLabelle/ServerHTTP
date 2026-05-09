using Network.HttpMessages;
using Network.Routing;
using System.IO.Compression;
using System.Text;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private delegate Stream GenerateCompressionStream(Stream output, CompressionMode direction);

        private static readonly Dictionary<string, GenerateCompressionStream> _streamGenerator = new()
        {
            { "gzip", (compressor, direction) => new GZipStream(compressor, direction) },
            { "br", (compressor, direction) => new BrotliStream(compressor, direction) },
            { "deflate", (compressor, direction) => new DeflateStream(compressor, direction) },
            { "zlib", (compressor, direction) => new ZLibStream(compressor, direction) }
        };

        private static async Task<byte[]> HandleCompressionAsync(byte[] body, GenerateCompressionStream streamBuilder, CompressionMode direction) 
        {
            using MemoryStream output = new();

            await using Stream compressor = streamBuilder(output, direction);
            await compressor.WriteAsync(body);

            return output.ToArray();
        }

        private static async Task<byte[]> OrganizeResponseBodyAsync(byte[] chunkedBody)
        {
            using MemoryStream memStream = new();
            int index = 0;
            while (index < chunkedBody.Length)
            {
                int chunkSizeEnd = Array.IndexOf(chunkedBody, (byte)'\r', index);
                if (chunkSizeEnd == -1)
                    break;

                string chunkSizeHex = Encoding.UTF8.GetString(chunkedBody, index, chunkSizeEnd - index);

                if (!int.TryParse(chunkSizeHex, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize) || chunkSize == 0)
                {
                    Console.WriteLine("Parsing error - Failed to obtain chunk size.");
                    break;
                }

                index = chunkSizeEnd + 2;
                await memStream.WriteAsync(chunkedBody.AsMemory(index, chunkSize));

                index += chunkSize + 2;
            }

            return memStream.ToArray();
        }

        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            byte[] body = requestContext.Request.Body;

            //organize chunked request
            if (requestContext.Request.Headers.TryGetValue("Transfer-Encoding", out string? transfer)
                && transfer.Contains("chunked"))
            {
                body = await OrganizeResponseBodyAsync(body);
            }

            //decode compressed request
            if (requestContext.Request.Headers.TryGetValue("Content-Encoding", out string? encoding)
                && _streamGenerator.TryGetValue(encoding, out GenerateCompressionStream? streamBuilder))
            {
                body = await HandleCompressionAsync(body, streamBuilder, CompressionMode.Decompress);
            }

            //rebuild request with organized and decompressed body
            requestContext = new RequestContext.Builder(requestContext)
                .SetRequest(
                    new Request.Builder(requestContext.Request)
                    .SetBody(body)
                    .Build()
                )
                .Build();

            return await next(requestContext);
        }
    }
}
