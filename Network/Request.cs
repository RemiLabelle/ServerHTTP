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
        _method = method;
        _path = path;
        _httpVersion = httpVersion;
    }

    public override byte[] GetMessage()
    {
        string requestLine = $"{_method} {_path} {_httpVersion}\r\n";

        return [.. Encoding.UTF8.GetBytes(requestLine), .. base.GetMessage()];
    }

    public string Method => _method;
    public string Path => _path;
    public string HttpVersion => _httpVersion;

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
