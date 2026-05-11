using System.Text;

namespace Network.HttpMessages;

public abstract class Message
{
    private readonly Dictionary<string, string> _headers;
    private readonly byte[] _body;
    private readonly string _httpVersion;

    public Message(Dictionary<string, string> headers, byte[] body, string httpVersion)
    {
        _body = (byte[])body.Clone();

        _headers = new(headers, StringComparer.OrdinalIgnoreCase);
        _headers.TryAdd("Content-Length", "");
        _headers["Content-Length"] = body.Length.ToString();

        _httpVersion = httpVersion;
    }

    public abstract byte[] GetMessage();

    public virtual byte[] GetHeadersBody()
    {
        MemoryStream stream = new();
        foreach (var (name, value) in _headers)
        {
            string header = $"{name}: {value}\r\n";
            stream.Write(Encoding.UTF8.GetBytes(header));
        }

        stream.Write(Encoding.UTF8.GetBytes("\r\n"));

        stream.Write(_body);

        return stream.ToArray();
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(GetMessage());
    }

    public IReadOnlyDictionary<string, string> Headers => _headers;
    public byte[] Body => (byte[])_body.Clone();
    public string HttpVersion => _httpVersion;

    public static Dictionary<string, string> ParseHeaders(string headersLines)
    {
        Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
        foreach (var line in headersLines.Split("\r\n"))
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;
            string name = line[..colonIndex].Trim();
            string value = line[(colonIndex + 1)..].Trim();
            headers.TryAdd(name, value);
        }
        return headers;
    }

    public abstract class Builder<TBuilder, TMessage>
        where TBuilder : Builder<TBuilder, TMessage>
        where TMessage : Message
    {
        protected Dictionary<string, string> _headers = new();
        protected byte[] _body = [];
        protected string _httpVersion = "HTTP/1.1";

        public Builder() { }

        public Builder(TMessage message)
        {
            _headers = new(message.Headers, StringComparer.OrdinalIgnoreCase);
            _body = message.Body;
            _httpVersion = message.HttpVersion;
        }

        public TBuilder SetHeaderHost(string value)
        {
            string name = "Host";
            if (!_headers.ContainsKey(value))
                _headers.Add(name, value);
            else
                _headers[name] = value;
            return (TBuilder)this;
        }

        public TBuilder SetHeaderContentType(string value)
        {
            string name = "Content-Type";
            if (!_headers.ContainsKey(value))
                _headers.Add(name, value);
            else
                _headers[name] = value;
            return (TBuilder)this;
        }

        public TBuilder SetHeaderServer(string value)
        {
            string name = "Server";
            if (!_headers.ContainsKey(value))
                _headers.Add(name, value);
            else
                _headers[name] = value;
            return (TBuilder)this;
        }

        public TBuilder AddHeader(string name, string value)
        {
            _headers.Add(name, value);
            return (TBuilder)this;
        }

        public TBuilder SetBody(byte[] body)
        {
            _body = body;
            return (TBuilder)this;
        }

        public TBuilder AddToBody(byte[] body)
        {
            _body = [.. _body, .. body];
            return (TBuilder)this;
        }

        public TBuilder SetHttpVersion(string httpVersion)
        {
            _httpVersion = httpVersion;
            return (TBuilder)this;
        }

        public virtual TBuilder Reset()
        {
            _headers.Clear();
            _body = [];
            return (TBuilder)this;
        }

        public abstract TMessage Build();
    }
}
