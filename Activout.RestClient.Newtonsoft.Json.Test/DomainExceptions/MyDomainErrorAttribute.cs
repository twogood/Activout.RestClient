using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Newtonsoft.Json.Test.DomainExceptions
{
    internal class MyDomainErrorAttribute : DomainErrorAttribute
    {
        public MyDomainErrorAttribute(MyApiError apiValue, MyDomainErrorEnum domainValue) : base(apiValue, domainValue)
        {
        }
    }
}