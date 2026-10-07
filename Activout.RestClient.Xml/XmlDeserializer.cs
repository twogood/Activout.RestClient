using Activout.RestClient.Serialization;

namespace Activout.RestClient.Xml;

public class XmlDeserializer : IDeserializer
{
    public int Order { get; set; } = 0;

    public async Task<object?> Deserialize(HttpContent content, Type type)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(type);
        var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var _ = stream.ConfigureAwait(false);
        return serializer.Deserialize(stream) ?? new object();
    }

    public bool CanDeserialize(MediaType mediaType)
    {
        return XmlHelper.SupportedMediaTypes.Contains(mediaType);
    }
}