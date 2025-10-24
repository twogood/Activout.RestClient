using System;

namespace Activout.RestClient;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public class ErrorResponseAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}