using System.Net;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal class MyDomainHttpErrorAttribute : DomainExceptions.DomainHttpErrorAttribute
    {
        public MyDomainHttpErrorAttribute(HttpStatusCode httpStatusCode, MyDomainErrorEnum domainErrorValue) : base(
            httpStatusCode, domainErrorValue)
        {
        }
    }
}