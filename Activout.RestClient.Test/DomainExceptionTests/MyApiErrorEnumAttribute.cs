using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal class MyApiErrorEnumAttribute : DomainErrorAttribute
    {
        public MyApiErrorEnumAttribute(MyApiError apiValue, MyDomainErrorEnum domainValue) : base(apiValue, domainValue)
        {
        }
    }
}