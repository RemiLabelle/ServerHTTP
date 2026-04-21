using Network.HttpMessages;
using Network.Routing;
using System.Text;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private static readonly Dictionary<string, Func<byte[]>> _dispatcher = new()
        {
            { "gzip", CompressAsGzip },
            { "br", CompressAsBrotli }
        };

        private static byte[] CompressAsGzip() 
        {
            return []; //todo
        }

        private static byte[] CompressAsBrotli() 
        {
            return []; //todo
        }

        public Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            new RequestContext.Builder(requestContext)
                .SetRequest(
                    new Request.Builder(requestContext.Request)
                    .SetBody(Encoding.UTF8.GetBytes("")) //todo
                    .Build()
                )
                .Build();

            return next(requestContext);
        }
    }
}
