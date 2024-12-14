namespace Activout.RestClient.Xml;

public static class XmlHelper
{
    public static readonly MediaType[] SupportedMediaTypes =
    [
        MediaType.ValueOf("application/xml"),
        MediaType.ValueOf("text/xml")
    ];
}