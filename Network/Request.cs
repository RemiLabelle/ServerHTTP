namespace Network;

public class Request : Message
{
    private string _method;
    private string _path;
    private string _httpVersion;

    public Request(string method, string path, Dictionary<string, string> headers, byte[] body, string httpVersion = "HTTP/1.1")
        : base(headers, body)
    {
        _method = method;
        _path = path;
        _httpVersion = httpVersion;
    }

    public string Method => _method;
    public string Path => _path;
    public string HttpVersion => _httpVersion;

    public class RequestBuilder
    {
        private string? _method;
        private string? _path;
        private string? _httpVersion;
        private Dictionary<string, string> _headers = new();
        private byte[] _body = [];

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

        public RequestBuilder AddHeader(string name, string value)
        {
            _headers.Add(name, value);
            return this;
        }

        public RequestBuilder SetBody(byte[] body)
        {
            _body = body;
            return this;
        }

        public RequestBuilder AddToBody(byte[] body)
        {
            _body = [.. _body, .. body];
            return this;
        }

        public RequestBuilder Cleanup()
        {
            _method = null;
            _path = null;
            _httpVersion = null;
            _headers = new();
            _body = [];

            return this;
        }

        public Request Build()
        {
            return new(_method ?? "GET", _path ?? "/", _headers, _body, _httpVersion ?? "HTTP/1.1");
        }
    }
}
