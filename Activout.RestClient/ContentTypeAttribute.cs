using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class ContentTypeAttribute : Attribute
{
    public string ContentType { get; }

    public ContentTypeAttribute(string contentType)
    {
        ContentType = contentType;
    }
}