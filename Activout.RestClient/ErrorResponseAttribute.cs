using System;
using System.Collections.Generic;
using System.Text;

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
