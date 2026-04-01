using System.Text;

namespace Network;

public class Response : Message
{
    private int _statusCode;
    private string _reasonPhrase;

    public Response(Dictionary<string, string> headers, byte[] body, string httpVersion, int statusCode, string reasonPhrase)
        : base(headers, body, httpVersion)
    {
        _statusCode = statusCode;
        _reasonPhrase = reasonPhrase;
    }

    public override byte[] GetMessage()
    {
        string statusLine = $"{HttpVersion} {StatusCode} {_reasonPhrase}\r\n";
        return [.. Encoding.UTF8.GetBytes(statusLine), .. base.GetMessage()];
    }

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

        result = new Response(headers, Encoding.UTF8.GetBytes(body), httpVersion, statusCode, reasonPhrase);
        return true;
    }


    public class ResponseBuilder : Builder<ResponseBuilder, Response>
    {
        protected int _statusCode = 200;
        protected string? _reasonPhrase;

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
            _statusCode = 200;
            _reasonPhrase = null;
            return base.Reset();
        }

        public override Response Build()
        {
            return new Response(_headers, _body, _httpVersion, _statusCode, _reasonPhrase ?? "OK");
        }
    }
}
