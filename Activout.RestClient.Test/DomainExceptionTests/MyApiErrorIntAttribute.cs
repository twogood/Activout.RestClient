using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal class MyApiErrorIntAttribute : DomainErrorAttribute
    {
        public MyApiErrorIntAttribute(int apiValue, MyDomainErrorEnum domainValue) : base(apiValue, domainValue)
        {
        }
    }
}