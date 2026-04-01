using System.Text;

namespace Network;

public abstract class Message
{
    private readonly Dictionary<string, string> _headers = new();
    private readonly byte[] _body = [];

    public Message(Dictionary<string, string> headers, byte[] body)
    {
        foreach (var (name, value) in headers)
            headers[name] = value.Trim();
        _headers = headers;
        _body = body;
    }

    public virtual byte[] GetMessage()
    {
        MemoryStream stream = new();
        foreach (var (name, value) in _headers)
        {
            string header = $"{name}: {value}\r\n";
            stream.Write(Encoding.UTF8.GetBytes(header));
        }

        string contentLengthHeader = $"Content-Length: {_body.Length}\r\n";
        stream.Write(Encoding.UTF8.GetBytes(contentLengthHeader));

        stream.Write(Encoding.UTF8.GetBytes("\r\n"));

        stream.Write(_body);

        return stream.ToArray();
    }

    public IReadOnlyDictionary<string, string> Headers => _headers;
    public byte[] Body => (byte[])_body.Clone();

    public abstract class MessageBuilder<TMessageBuilder, TMessage>
        where TMessageBuilder : MessageBuilder<TMessageBuilder, TMessage>
        where TMessage : Message
    {
        protected Dictionary<string, string> _headers = new();
        protected byte[] _body = [];

        public TMessageBuilder AddHeader(string name, string value)
        {
            _headers.Add(name, value);
            return (TMessageBuilder)this;
        }

        public TMessageBuilder SetBody(byte[] body)
        {
            _body = body;
            return (TMessageBuilder)this;
        }

        public TMessageBuilder AddToBody(byte[] body)
        {
            _body = [.. _body, .. body];
            return (TMessageBuilder)this;
        }
        
        public virtual TMessageBuilder Reset()
        {
            _headers.Clear();
            _body = [];
            return (TMessageBuilder)this;
        }

        public abstract TMessage Build();
    }
}
