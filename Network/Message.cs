namespace Network;

public abstract class Message
{
    private Dictionary<string, string> _headers = new();
    private byte[] _body = [];

    public Message(Dictionary<string, string> headers, byte[] body)
    {
        _headers = headers;
        _body = body;
    }

    public IReadOnlyDictionary<string, string> Headers => _headers;
    public byte[] Body => (byte[])_body.Clone();
}
