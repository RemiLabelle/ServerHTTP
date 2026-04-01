using System.Text;

namespace Network;

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

    public virtual byte[] GetMessage()
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

    public IReadOnlyDictionary<string, string> Headers => _headers;
    public byte[] Body => (byte[])_body.Clone();
    public string HttpVersion => _httpVersion;

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
