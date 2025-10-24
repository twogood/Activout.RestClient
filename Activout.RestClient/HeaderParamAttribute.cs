using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderParamAttribute(string? name = null, bool replace = true) : NamedParamAttribute(name)
{
    public bool Replace { get; } = replace;
}