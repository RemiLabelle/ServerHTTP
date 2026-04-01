using System.Text;

namespace Network;

public class Request : Message
{
    private readonly string _method;
    private readonly string _path;
    private readonly string _httpVersion;

    public Request(Dictionary<string, string> headers, byte[] body, string method, string path, string httpVersion = "HTTP/1.1")
        : base(headers, body)
    {
        _method = method.Trim();
        _path = path.Trim();
        _httpVersion = httpVersion.Trim();
    }

    public override byte[] GetMessage()
    {
        string requestLine = $"{_method} {_path} {_httpVersion}\r\n";

        return [.. Encoding.UTF8.GetBytes(requestLine), .. base.GetMessage()];
    }

    public string Method => _method;
    public string Path => _path;
    public string HttpVersion => _httpVersion;

    public static bool TryParse(string input, out Request? result)
    {
        string separator = "\r\n";
        int requestLineIdx = input.IndexOf(separator);
        if (requestLineIdx == -1)
        {
            result = null;
            return false;
        }

        string requestLine = input.Substring(0, requestLineIdx);

        string[] requestLineparts = requestLine.Split(" ");

        if (requestLineparts.Length != 3)
        {
            result = null;
            return false;
        }

        string method = requestLineparts[0];
        string path = requestLineparts[1];
        string httpVersion = requestLineparts[2];


        int headersEndIdx = input.IndexOf(separator + separator);
        if (headersEndIdx == -1)
        {
            result = null;
            return false;
        }

        int headerStartIdx = requestLineIdx + separator.Length;
        string headersLines = input.Substring(headerStartIdx, headersEndIdx - headerStartIdx);
        int bodyStartIdx = headersEndIdx + separator.Length * 2;
        string body = input.Substring(bodyStartIdx);

        Dictionary<string, string> headers = new();
        foreach (string header in headersLines.Split(separator))
        {
            string[] nameValue = header.Split(":");
            if (nameValue.Length != 2)
            {
                result = null;
                return false;
            }

            headers.Add(nameValue[0], nameValue[1]);
        }

        result = new Request(headers, Encoding.UTF8.GetBytes(body), method, path, httpVersion);
        return true;
    }

    public class RequestBuilder : MessageBuilder<RequestBuilder, Request>
    {
        protected string? _method;
        protected string? _path;
        protected string? _httpVersion;

        public RequestBuilder SetMethod(string method)
        {
            _method = method;
            return this;
        }

        public RequestBuilder SetPath(string path)
        {
            _path = path;
            return this;
        }

        public RequestBuilder SetHttpVersion(string httpVersion)
        {
            _httpVersion = httpVersion;
            return this;
        }

        public override RequestBuilder Reset()
        {
            _method = null;
            _path = null;
            _httpVersion = null;
            return base.Reset();
        }

        public override Request Build()
        {
            return new(_headers, _body, _method ?? "GET", _path ?? "/", _httpVersion ?? "HTTP/1.1");
        }
    }
}
