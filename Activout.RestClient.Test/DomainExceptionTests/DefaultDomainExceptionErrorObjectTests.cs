using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.DomainExceptions;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test.DomainExceptionTests;

[ErrorResponse(typeof(MyApiErrorResponse))]
[DomainException(typeof(SomeDomainErrorObjectException))]
[DomainHttpError(HttpStatusCode.BadRequest, MyDomainErrorEnum.ClientError)]
[DomainHttpError(HttpStatusCode.InternalServerError, MyDomainErrorEnum.ServerError)]
public interface ISomeApiErrorObjectClient
{
    Task Api();
}

internal class SomeDomainErrorObjectException : Exception
{
    public MyDomainErrorEnum Error { get; }

    public SomeDomainErrorObjectException(MyDomainErrorEnum error, Exception innerException = null) : base(
        error.ToString(), innerException)
    {
        Error = error;
    }
}

public class DefaultDomainExceptionErrorObjectTests
{
    private const string BaseUri = "https://example.com";

    private readonly MockHttpMessageHandler _mockHttp;
    private readonly ISomeApiErrorObjectClient _defaultMapperApiClient;

    public DefaultDomainExceptionErrorObjectTests()
    {
        _mockHttp = new MockHttpMessageHandler();

        _defaultMapperApiClient = Services.CreateRestClientFactory()
            .CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri))
            .Build<ISomeApiErrorObjectClient>();
    }

    [Fact]
    public async Task TestMapApiErrorObject()
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(_ => JsonHttpResponseMessage(HttpStatusCode.BadRequest, MyApiError.Foo));

        // Act
        var exception = await Assert.ThrowsAsync<SomeDomainErrorObjectException>(() =>
            _defaultMapperApiClient.Api());

        // Assert
        Assert.Equal(MyDomainErrorEnum.DomainFoo, exception.Error);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, MyDomainErrorEnum.ClientError)]
    [InlineData(HttpStatusCode.BadGateway, MyDomainErrorEnum.ServerError)]
    public async Task TestDeserializerException(HttpStatusCode httpStatusCode, MyDomainErrorEnum error)
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(_ => HtmlHttpResponseMessage(httpStatusCode));

        // Act
        var exception = await Assert.ThrowsAsync<SomeDomainErrorObjectException>(() =>
            _defaultMapperApiClient.Api());

        // Assert
        Assert.Equal(error, exception.Error);
        Assert.IsType<MissingMethodException>(exception.InnerException);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, MyDomainErrorEnum.ClientError)]
    [InlineData(HttpStatusCode.BadGateway, MyDomainErrorEnum.ServerError)]
    public async Task TestNoDeserializerFound(HttpStatusCode httpStatusCode, MyDomainErrorEnum error)
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(_ => FoobarHttpResponseMessage(httpStatusCode));

        // Act
        var exception = await Assert.ThrowsAsync<SomeDomainErrorObjectException>(() =>
            _defaultMapperApiClient.Api());

        // Assert
        Assert.Equal(error, exception.Error);
        Assert.IsType<RestClientException>(exception.InnerException);
    }

    private static HttpResponseMessage JsonHttpResponseMessage(HttpStatusCode httpStatusCode, MyApiError myApiError)
    {
        return new HttpResponseMessage(httpStatusCode)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new MyApiErrorResponse
            {
                Code = myApiError
            }), Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage HtmlHttpResponseMessage(HttpStatusCode httpStatusCode)
    {
        return new HttpResponseMessage(httpStatusCode)
        {
            Content = new StringContent($"<html><head><title>Error {httpStatusCode}</title></head></html>",
                Encoding.UTF8, "text/html")
        };
    }

    private static HttpResponseMessage FoobarHttpResponseMessage(HttpStatusCode httpStatusCode)
    {
        return new HttpResponseMessage(httpStatusCode)
        {
            Content = new StringContent("foobar", Encoding.UTF8, "application/foobar")
        };
    }
}