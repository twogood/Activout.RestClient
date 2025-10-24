using static Activout.RestClient.Xml.XmlHelper;

namespace Activout.RestClient.Xml;

public static class RestClientBuilderXmlExtensions
{
    public static IRestClientBuilder WithXml(this IRestClientBuilder builder)
    {
        return builder
            .With(new XmlSerializer())
            .With(new XmlDeserializer())
            .Accept(string.Join(", ", SupportedMediaTypes.Select(type => type.Value)))
            .ContentType(SupportedMediaTypes.First());
    }
}