namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal enum MyDomainErrorEnum
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