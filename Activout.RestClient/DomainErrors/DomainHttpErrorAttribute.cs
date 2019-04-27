using System;
using System.Net;

namespace Activout.RestClient.DomainErrors
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class DomainHttpErrorAttribute : Attribute
    {
        public HttpStatusCode HttpStatusCode { get; }
        public object DomainErrorValue { get; }

        public DomainHttpErrorAttribute(HttpStatusCode httpStatusCode, object domainErrorValue)
        {
            HttpStatusCode = httpStatusCode;
            DomainErrorValue = domainErrorValue;
        }
    }
}