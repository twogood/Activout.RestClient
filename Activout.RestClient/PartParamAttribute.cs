using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Parameter)]
public class PartParamAttribute(string? name = null, string? fileName = null, string? contentType = null)
    : NamedParamAttribute(name)
{
    public string? FileName { get; } = fileName;
    public MediaType? ContentType { get; } = MediaType.ValueOf(contentType);
}