#nullable disable
namespace Activout.RestClient.Newtonsoft.Json.Test.DomainExceptions
{
    public class MyApiErrorResponse
    {
        [MyDomainError(MyApiError.Foo, MyDomainErrorEnum.DomainFoo)]
        public MyApiError Code { get; set; }
    }
}