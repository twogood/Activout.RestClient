using Activout.RestClient.DomainErrors;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    public class MyApiErrorResponse
    {
        [DomainError(MyApiError.Foo, MyDomainErrorEnum.DomainFoo)]
        public MyApiError Code { get; set; }
    }
}