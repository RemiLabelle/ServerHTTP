using System.Text;

namespace Network;

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
