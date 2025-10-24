#nullable disable
using System;

namespace Activout.RestClient
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ErrorResponseAttribute : Attribute
    {
        public ErrorResponseAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}