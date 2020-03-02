namespace Activout.RestClient.Test.DomainExceptionTests
{
    public class MyApiIntErrorResponse
    {
        [MyApiErrorInt((int)MyApiError.Foo, MyDomainErrorEnum.DomainFoo)]
        public int? Code { get; set; }
    }
}