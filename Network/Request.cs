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
        string requestLine = $"{_method} {_path} {FormatedHttpVersion}\r\n";

        return [.. Encoding.UTF8.GetBytes(requestLine), .. base.GetMessage()];
    }

    public string Method => _method;
    public string Path => _path;

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

        result = new Request(headers, Encoding.UTF8.GetBytes(body), httpVersion, method, path);
        return true;
    }

    public class RequestBuilder : Builder<RequestBuilder, Request>
    {
        protected string? _method;
        protected string? _path;

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

        public override RequestBuilder Reset()
        {
            _method = null;
            _path = null;
            return base.Reset();
        }

        public override Request Build()
        {
            return new(_headers, _body, _method ?? "GET", _path ?? "/", _httpVersion ?? "HTTP/1.1");
        }
    }
}
