using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Parameter)]
public class HeaderParamAttribute : NamedParamAttribute
{
    public bool Replace { get; }

    public HeaderParamAttribute(string name = null, bool replace = true) : base(name)
    {
        Replace = replace;
    }
}