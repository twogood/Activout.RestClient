using System.Text;
using System.Xml;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Xml;

public class XmlSerializer : ISerializer
{
    public int Order { get; set; } = 0;

    public XmlWriterSettings? Settings { get; set; }

    public HttpContent Serialize(object? data, Encoding encoding, MediaType mediaType)
    {
        if (data == null)
        {
            return new StringContent("", encoding, mediaType.Value);
        }

        var serializer = new System.Xml.Serialization.XmlSerializer(data.GetType());
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        var xmlWriter = XmlWriter.Create(writer, Settings);
        serializer.Serialize(xmlWriter, data);
        stream.Seek(0, SeekOrigin.Begin);
        return new StreamContent(stream);
    }

    public bool CanSerialize(MediaType mediaType)
    {
        return XmlHelper.SupportedMediaTypes.Contains(mediaType);
    }
}