using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public class HeaderAttribute : Attribute
{
    public string Name { get; }
    public string Value { get; }
    public bool Replace { get; }

    public HeaderAttribute(string name, string value, bool replace = true)
    {
        Name = name;
        Value = value;
        Replace = replace;
    }
}