using Network.HttpMessages;
using Network.Routing;
using System.Text;

namespace Network.Handlers;

public class ServeStaticFileHandler : IHandler
{
    public string AbsolutePath { get; private set; }

    public ServeStaticFileHandler(string absolutePath)
    {
        AbsolutePath = absolutePath;
    }

    public IHandler.Handler GetHandler()
    {
        return async (requestContext) =>
        {
            string requestedFile = requestContext.Request.Path[requestContext.Prefix.Length..];
            string absoluteRequestedFile = Path.GetFullPath(Path.Combine(AbsolutePath, requestedFile));
            if (!absoluteRequestedFile.StartsWith(AbsolutePath))
                return new Response.Builder()
                    .SetStatusCode(400)
                    .SetBody(Encoding.UTF8.GetBytes(@"{""status"": ""Cannot use path traversal.""}"))
                    .Build();
            if (!File.Exists(absoluteRequestedFile))
                return new Response.Builder()
                    .SetStatusCode(404)
                    .SetBody(Encoding.UTF8.GetBytes(@"{""status"": ""File not found.""}"))
                    .Build();
            byte[] fileContent = await File.ReadAllBytesAsync(absoluteRequestedFile);

            string contentType = "text/plain";
            string fileExtension = Path.GetExtension(absoluteRequestedFile)[1..].ToLower();

            switch (fileExtension)
            {
                case "css":
                    contentType = "text/css";
                    break;
                case "csv":
                    contentType = "text/csv";
                    break;
                case "html":
                    contentType = "text/html";
                    break;
                case "js":
                    contentType = "text/javascript";
                    break;
                case "xml":
                    contentType = "text/sml";
                    break;
            }

            return new Response.Builder()
                .SetStatusCode(200)
                .SetHeaderContentType(contentType)
                .SetBody(fileContent)
                .Build();
        };
    }
}
