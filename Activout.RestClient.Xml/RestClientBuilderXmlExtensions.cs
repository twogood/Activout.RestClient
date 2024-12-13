namespace Activout.RestClient.Xml;

public static class RestClientBuilderXmlExtensions
{
    public static IRestClientBuilder WithXml(this IRestClientBuilder builder)
    {
        return builder
            .With(new XmlSerializer())
            .With(new XmlDeserializer())
            .Accept("text/xml")
            .ContentType("text/xml");
    }
}