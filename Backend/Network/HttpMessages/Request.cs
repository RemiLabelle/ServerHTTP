using System.Text;

namespace Network.HttpMessages;

public class Request : Message
{
    private readonly string _method;
    private readonly string _path;

    public Request(Dictionary<string, string> headers, byte[] body, string httpVersion, string method, string path)
        : base(headers, body, httpVersion)
    {
        _method = method;
        _path = path;
    }

    public override byte[] GetMessage()
    {
        string requestLine = $"{_method} {_path} {HttpVersion}\r\n";

        return [.. Encoding.UTF8.GetBytes(requestLine), .. GetHeadersBody()];
    }

    public Dictionary<string, string> Cookies()
    {
        if (!Headers.TryGetValue("Cookie", out string? rawCookies))
            return new Dictionary<string, string>();
        Dictionary<string, string> cookies = new();
        foreach (string cookie in rawCookies.Split(";"))
        {
            int equalsIndex = cookie.IndexOf('=');
            if (equalsIndex <= 0) continue;
            string name = cookie[..equalsIndex].Trim();
            string value = cookie[(equalsIndex + 1)..].Trim();
            cookies.TryAdd(name, value);
        }
        return cookies;
    }

    public Dictionary<string, string> GetParams()
    {
        string[] pathParts = Path.Split("?");
        if (pathParts.Length < 2)
            return new Dictionary<string, string>();
        string[] allParams = pathParts[1].Split("&");

        Dictionary<string, string> keyValueParams = new();
        foreach (string param in allParams)
        {
            string[] paramParts = param.Split("=");
            if (paramParts.Length == 2)
                continue;
            keyValueParams.TryAdd(paramParts[0], paramParts[1]);
        }

        return keyValueParams;
    }

    public string Method => _method;
    public string Path => _path;

    public static Request Parse(string input)
    {
        string requestLine = input.Split("\r\n")[0];
        string headerLines = string.Join("\r\n", input.Split("\r\n\r\n")[0].Split("\r\n")[1..]);
        string bodyText = string.Join("\r\n\r\n", input.Split("\r\n\r\n")[1..]);

        string[] requestLineParts = requestLine.Split(' ');

        string method = requestLineParts[0];
        string path = requestLineParts[1];
        string httpVersion = requestLineParts[2];

        Dictionary<string, string> headers = ParseHeaders(headerLines);

        byte[] body = Encoding.UTF8.GetBytes(bodyText);

        return new Request(headers, body, httpVersion, method, path);
    }

    public static bool TryParse(string input, out Request? result)
    {
        try
        {
            result = Parse(input);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public class Builder : Message.Builder<Builder, Request>
    {
        protected string? _method;
        protected string? _path;

        public Builder() { }

        public Builder(Request request) : base(request)
        {
            _method = request.Method;
            _path = request.Path;
        }

        public Builder SetMethod(string method)
        {
            _method = method;
            return this;
        }

        public Builder SetPath(string path)
        {
            _path = path;
            return this;
        }

        public override Builder Reset()
        {
            _method = null;
            _path = null;
            return base.Reset();
        }

        public override Request Build()
        {
            return new(_headers, _body, _httpVersion, _method ?? "GET", _path ?? "/");
        }
    }
}
