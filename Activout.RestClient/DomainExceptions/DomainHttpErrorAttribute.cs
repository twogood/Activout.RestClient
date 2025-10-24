using System;
using System.Net;

namespace Activout.RestClient.DomainExceptions;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public class DomainHttpErrorAttribute(HttpStatusCode httpStatusCode, object domainErrorValue) : Attribute
{
    public HttpStatusCode HttpStatusCode { get; } = httpStatusCode;
    public object DomainErrorValue { get; } = domainErrorValue;
}