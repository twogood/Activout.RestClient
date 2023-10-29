namespace Activout.RestClient.Test.DomainExceptionTests;

public class MyApiErrorResponse
{
    [MyDomainError(MyApiError.Foo, MyDomainErrorEnum.DomainFoo)]
    public MyApiError Code { get; set; }
}