using System.Net;
using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Test.DomainExceptionTests;

internal class MyDomainHttpErrorAttribute : DomainHttpErrorAttribute
{
    public MyDomainHttpErrorAttribute(HttpStatusCode httpStatusCode, MyDomainErrorEnum domainErrorValue) : base(
        httpStatusCode, domainErrorValue)
    {
    }
}