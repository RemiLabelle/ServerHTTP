using System.Text;

namespace Network;

public class Response : Message
{
    private readonly int _statusCode;
    private readonly string _reasonPhrase;

    public Response(Dictionary<string, string> headers, byte[] body, string httpVersion, int statusCode, string reasonPhrase)
        : base(headers, body, httpVersion)
    {
        _statusCode = statusCode;
        _reasonPhrase = reasonPhrase;
    }

    public override byte[] GetMessage()
    {
        string statusLine = $"{HttpVersion} {StatusCode}{(_reasonPhrase == string.Empty ? string.Empty : " " + _reasonPhrase)}\r\n";
        return [.. Encoding.UTF8.GetBytes(statusLine), .. GetHeadersBody()];
    }

    public int StatusCode => _statusCode;
    public string ReasonPhrase => _reasonPhrase;

    public static Response Parse(string input)
    {
        string statusLine = input.Split("\r\n")[0];
        string headerLines = string.Join("\r\n", input.Split("\r\n\r\n")[0].Split("\r\n")[1..]);
        string bodyText = string.Join("\r\n\r\n", input.Split("\r\n\r\n")[1..]);

        string[] requestLineParts = statusLine.Split(' ');

        string httpVersion = requestLineParts[0];
        int statusCode = int.Parse(requestLineParts[1]);
        string reasonPhrase = string.Join(" ", requestLineParts[2..]);

        Dictionary<string, string> headers = ParseHeaders(headerLines);

        byte[] body = Encoding.UTF8.GetBytes(bodyText);

        return new Response(headers, body, httpVersion, statusCode, reasonPhrase);
    }

    public static bool TryParse(string input, out Response? result)
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

    public class Builder : Message.Builder<Builder, Response>
    {
        protected int _statusCode = 200;
        protected string? _reasonPhrase;

        public Builder SetStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public Builder SetReasonPhrase(string reasonPhrase)
        {
            _reasonPhrase = reasonPhrase;
            return this;
        }

        public override Builder Reset()
        {
            _statusCode = 200;
            _reasonPhrase = null;
            return base.Reset();
        }

        public override Response Build()
        {
            return new Response(_headers, _body, _httpVersion, _statusCode, _reasonPhrase ?? NetworkUtils.GetDefaultReasonPhrase(_statusCode));
        }
    }
}
