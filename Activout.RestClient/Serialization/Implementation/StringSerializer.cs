using System.Net.Http;
using System.Text;

namespace Activout.RestClient.Serialization.Implementation;

public class StringSerializer : ISerializer
{
    public static ISerializer Instance { get; } = new StringSerializer();

    public int Order { get; set; } = 1000;

    public HttpContent Serialize(object? data, Encoding encoding, MediaType mediaType)
    {
        return new StringContent(data?.ToString() ?? "", encoding, mediaType.Value);
    }

    public bool CanSerialize(MediaType mediaType)
    {
        return mediaType.Value.StartsWith("text/");
    }
}