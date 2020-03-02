namespace Activout.RestClient.Test.DomainExceptionTests
{
    public class MyApiEnumErrorResponse
    {
        [MyApiErrorEnum(MyApiError.Foo, MyDomainErrorEnum.DomainFoo)]
        public MyApiError Code { get; set; }
    }
}