using System;

namespace Activout.RestClient.DomainExceptions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DomainErrorAttribute : Attribute
    {
        public object ApiValue { get; }
        public object DomainValue { get; }

        public DomainErrorAttribute(object apiValue, object domainValue)
        {
            ApiValue = apiValue;
            DomainValue = domainValue;
        }
    }
}