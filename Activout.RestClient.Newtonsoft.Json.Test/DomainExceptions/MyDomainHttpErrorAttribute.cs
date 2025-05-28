using System.Net;
using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Newtonsoft.Json.Test.DomainExceptions
{
    internal class MyDomainHttpErrorAttribute : DomainHttpErrorAttribute
    {
        public MyDomainHttpErrorAttribute(HttpStatusCode httpStatusCode, MyDomainErrorEnum domainErrorValue) : base(
            httpStatusCode, domainErrorValue)
        {
        }
    }
}