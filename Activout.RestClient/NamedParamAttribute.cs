using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class NamedParamAttribute(string? name) : Attribute
{
    public string? Name { get; } = name;
}