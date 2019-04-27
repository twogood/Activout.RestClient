using System;

namespace Activout.RestClient.DomainErrors
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class DomainExceptionAttribute : Attribute
    {
        public Type ExceptionType { get; }
        public Type ErrorType { get; }

        public DomainExceptionAttribute(Type exceptionType)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException("Type must be an exception", nameof(exceptionType));
            }

            var errorProperty = exceptionType.GetProperty("Error") ??
                                throw new ArgumentException("Exception must have a property called Error",
                                    nameof(exceptionType));

            ExceptionType = exceptionType;
            ErrorType = errorProperty.PropertyType;
        }
    }
}