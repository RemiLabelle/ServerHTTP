using Network.HttpMessages;
using Network.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Network.Handlers.IHandler;

namespace Network.Middlewares
{
    public class CorsMiddleware : IMiddleware
    {
        public async Task<Response> Handle(RequestContext requestContext, Handler next)
        {
            string origin = requestContext.Request.Headers.TryGetValue("Origin", out string? originHeader) ? originHeader : "*";

            if (requestContext.Request.Method == "OPTIONS")
            {
                return new Response.Builder()
                    .SetStatusCode(204)
                    .AddHeader("Access-Control-Allow-Origin", origin)
                    .AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
                    .AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization")
                    .AddHeader("Access-Control-Allow-Credentials", "true")
                    .Build();
            }

            Response response = await next(requestContext);

            return new Response.Builder(response)
                .AddHeader("Access-Control-Allow-Origin", origin)
                .AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS")
                .AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization")
                .AddHeader("Access-Control-Allow-Credentials", "true")
                .Build();
        }
    }
}
