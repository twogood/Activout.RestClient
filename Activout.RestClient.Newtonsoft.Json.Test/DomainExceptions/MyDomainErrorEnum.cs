#nullable disable
namespace Activout.RestClient.Newtonsoft.Json.Test.DomainExceptions
{
    public enum MyDomainErrorEnum
    {
        Unknown = 0,
        AccessDenied,
        Forbidden,
        DomainFoo,
        DomainBar,
        ServerError,
        ClientError
    }
}