using Activout.RestClient.DomainExceptions;

namespace Activout.RestClient.Test.DomainExceptionTests;

internal class MyDomainErrorAttribute : DomainErrorAttribute
{
    public MyDomainErrorAttribute(MyApiError apiValue, MyDomainErrorEnum domainValue) : base(apiValue, domainValue)
    {
    }
}