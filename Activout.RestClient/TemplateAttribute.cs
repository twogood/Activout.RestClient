using System;

namespace Activout.RestClient;

public abstract class TemplateAttribute(string? template = null) : Attribute
{
    public string? Template { get; } = template;
}