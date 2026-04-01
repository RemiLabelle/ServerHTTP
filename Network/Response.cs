using System.Text;

namespace Network;

public class Response : Message
{
    private string _httpVersion;
    private int _statusCode;
    private string _reasonPhrase;

    public Response(Dictionary<string, string> headers, byte[] body, int statusCode, string reasonPhrase, string httpVersion = "HTTP/1.1")
        : base(headers, body)
    {
        _httpVersion = httpVersion.Trim();
        _statusCode = statusCode;
        _reasonPhrase = reasonPhrase.Trim();
    }

    public override byte[] GetMessage()
    {
        string statusLine = $"{_httpVersion} {StatusCode} {_reasonPhrase}\r\n";
        return [.. Encoding.UTF8.GetBytes(statusLine), .. base.GetMessage()];
    }

    public string HttpVersion => _httpVersion;
    public int StatusCode => _statusCode;
    public string ReasonPhrase => _reasonPhrase;

    public static bool TryParse(string input, out Response? result)
    {
        string separator = "\r\n";
        int responseLineIdx = input.IndexOf(separator);
        if (responseLineIdx == -1)
        {
            result = null;
            return false;
        }

        string responseLine = input.Substring(0, responseLineIdx);

        string[] responseLineParts = responseLine.Split(" ");

        if (responseLineParts.Length < 2)
        {
            result = null;
            return false;
        }

        string httpVersion = responseLineParts[0];
        if (!int.TryParse(responseLineParts[1], out int statusCode))
        {
            result = null;
            return false;
        }
        string reasonPhrase = responseLineParts.Length > 2 ? string.Join(" ", responseLineParts[2..]) : string.Empty;


        int headersEndIdx = input.IndexOf(separator + separator);
        if (headersEndIdx == -1)
        {
            result = null;
            return false;
        }

        int headerStartIdx = responseLineIdx + separator.Length;
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

        result = new Response(headers, Encoding.UTF8.GetBytes(body), statusCode, reasonPhrase, httpVersion);
        return true;
    }


    public class ResponseBuilder : MessageBuilder<ResponseBuilder, Response>
    {
        protected string _httpVersion = "HTTP/1.1";
        protected int _statusCode = 200;
        protected string? _reasonPhrase;

        public ResponseBuilder SetHttpVersion(string httpVersion)
        {
            _httpVersion = httpVersion;
            return this;
        }

        public ResponseBuilder SetStatusCode(int statusCode)
        {
            _statusCode = statusCode;
            return this;
        }

        public ResponseBuilder SetReasonPhrase(string reasonPhrase)
        {
            _reasonPhrase = reasonPhrase;
            return this;
        }

        public override ResponseBuilder Reset()
        {
            _httpVersion = "HTTP/1.1";
            _statusCode = 200;
            _reasonPhrase = null;
            return base.Reset();
        }

        public override Response Build()
        {
            return new Response(_headers, _body, _statusCode, _reasonPhrase ?? "OK", _httpVersion);
        }
    }
}
