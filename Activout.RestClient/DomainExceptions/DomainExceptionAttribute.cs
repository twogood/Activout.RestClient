#nullable disable
using System;

namespace Activout.RestClient.DomainExceptions
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DomainExceptionAttribute : Attribute
    {
        public Type Type { get; }

        public DomainExceptionAttribute(Type type)
        {
            if (!typeof(Exception).IsAssignableFrom(type))
            {
                throw new ArgumentException("Type must be an exception: " + type, nameof(type));
            }

            Type = type;
        }
    }
}