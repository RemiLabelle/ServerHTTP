using System.ComponentModel;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Network;

public class NetworkUtils
{
    public static string GetDefaultReasonPhrase(int statusCode)
    {
        return statusCode switch
        {
            100 => "Continue",
            101 => "Switching Protocols",
            102 => "Processing",
            103 => "Early Hints",
            200 => "Success",
            201 => "Created",
            202 => "Accepted",
            203 => "Non-Authoritative Information",
            204 => "No Content",
            205 => "Reset Content",
            206 => "Partial Content",
            207 => "Multi-Status",
            208 => "Already Reported",
            226 => "IM Used",
            300 => "Multiple Choices",
            301 => "Moved Permanently",
            302 => "Found",
            303 => "See Other",
            304 => "Not Modified",
            305 => "Use Proxy",
            306 => "unused",
            307 => "Temporary Redirect",
            308 => "Permanent Redirect",
            400 => "Bad Request",
            401 => "Unauthorized",
            402 => "Payment Required",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            407 => "Proxy Authentication Required",
            408 => "Request Timeout",
            409 => "Conflict",
            410 => "Gone",
            411 => "Length Required",
            412 => "Precondition Failed",
            413 => "Request Entity Too Large",
            414 => "Request-URI Too Long",
            415 => "Unsupported Media Type",
            416 => "Requested Range Not Satisfiable",
            417 => "Expectation Failed",
            418 => "I'm a teapot",
            419 => "Insufficient Space on Resource",
            420 => "Method Failure",
            421 => "Misdirected Request",
            422 => "Unprocessable Entity",
            423 => "Locked",
            424 => "Failed Dependency",
            426 => "Upgrade Required",
            428 => "Precondition Required",
            429 => "Too Many Requests",
            431 => "Request Header Fields Too Large",
            451 => "Unavailable For Legal Reasons",
            500 => "Internal Server Error",
            501 => "Not Implemented",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            505 => "HTTP Version Not Supported",
            507 => "Insufficient Storage",
            511 => "Network Authentication Required",
            _ => string.Empty,
        };
    }

    public delegate Stream GenerateCompressionStream(Stream output, CompressionMode direction);

    public static async Task<byte[]> HandleCompressionAsync(byte[] body, GenerateCompressionStream streamBuilder, CompressionMode direction)
    {
        if (direction == CompressionMode.Compress)
        {
            using MemoryStream output = new();

            await using (Stream compressor = streamBuilder(output, direction))
            {
                await compressor.WriteAsync(body);
            }

            return output.ToArray();
        }
        else 
        {
            using MemoryStream input = new(body);
            await using Stream decompressor = streamBuilder(input, direction);
            using MemoryStream output = new();

            await decompressor.CopyToAsync(output);

            return output.ToArray();
        }
    }

    public static async Task<byte[]> ChunkBodyAsync(byte[] body, int chunkSize = 5) 
    {
        using MemoryStream output = new();
        int index = 0;

        while (index < body.Length) 
        {
            int size = Math.Min(chunkSize, body.Length - index);

            await output.WriteAsync(Encoding.UTF8.GetBytes(size.ToString("X")));
            await output.WriteAsync(Encoding.UTF8.GetBytes("\r\n"));
            await output.WriteAsync(body.AsMemory(index, size));
            await output.WriteAsync(Encoding.UTF8.GetBytes("\r\n"));
            index += size;
        }

        await output.WriteAsync(Encoding.UTF8.GetBytes("0\r\n\r\n"));

        return output.ToArray();
    }

    public static async Task<byte[]> DechunkBodyAsync(byte[] chunkedBody)
    {
        using MemoryStream memStream = new();
        int index = 0;
        while (index < chunkedBody.Length)
        {
            int chunkSizeEnd = Array.IndexOf(chunkedBody, (byte)'\r', index);
            if (chunkSizeEnd == -1)
                break;

            string chunkSizeHex = Encoding.UTF8.GetString(chunkedBody, index, chunkSizeEnd - index);

            if (!int.TryParse(chunkSizeHex, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize))
            {
                Console.WriteLine("Parsing error - Failed to obtain chunk size.");
                break;
            }

            if (chunkSize == 0)
                break;

            index = chunkSizeEnd + 2;
            await memStream.WriteAsync(chunkedBody.AsMemory(index, chunkSize));

            index += chunkSize + 2;
        }

        return memStream.ToArray();
    }
}