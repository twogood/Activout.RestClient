using System;
using System.Net.Http;
using System.Text;

namespace Activout.RestClient.Serialization.Implementation;

public sealed class StringSerializer : ISerializer
{
    public static ISerializer Instance { get; } = new StringSerializer();

    public int Order { get; set; } = 1000;

    public HttpContent Serialize(object? data, Encoding encoding, MediaType mediaType) =>
        new StringContent(data?.ToString() ?? "", encoding, mediaType.Value);

    public bool CanSerialize(MediaType mediaType) =>
        mediaType.Value.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
}