using System;

namespace Activout.RestClient.DomainExceptions
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DomainExceptionAttribute : Attribute
    {
        public Type ExceptionType { get; }

        public DomainExceptionAttribute(Type exceptionType)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException("Type must be an exception", nameof(exceptionType));
            }

            ExceptionType = exceptionType;
        }
    }
}