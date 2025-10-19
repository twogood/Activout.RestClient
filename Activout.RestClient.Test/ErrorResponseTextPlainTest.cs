using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test;

public class ErrorResponseTextPlainTest(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/api";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private ITestService CreateTestService()
    {
        return _restClientFactory.CreateBuilder()
            .With(_loggerFactory.CreateLogger<ErrorResponseTextPlainTest>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<ITestService>();
    }

    private ITestServiceWithCustomError CreateTestServiceWithCustomError()
    {
        return _restClientFactory.CreateBuilder()
            .With(_loggerFactory.CreateLogger<ErrorResponseTextPlainTest>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<ITestServiceWithCustomError>();
    }

    [Fact]
    public async Task TestErrorResponse_TextPlain_BadRequest_Async()
    {
        // arrange
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/resource")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Invalid request parameter");

        var service = CreateTestService();

        // act
        var exception = await Assert.ThrowsAsync<RestClientException>(() => service.GetResourceAsync());

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.ErrorResponse);
        Assert.IsType<string>(exception.ErrorResponse);
        Assert.Equal("Invalid request parameter", exception.GetErrorResponse<string>());
        Assert.Equal("Invalid request parameter", exception.Message);
    }

    [Fact]
    public async Task TestErrorResponse_TextPlain_BadRequest_CustomErrorMessage_Async()
    {
        // arrange
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/resource")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Invalid request parameter");

        var service = CreateTestServiceWithCustomError();

        // act
        var exception = await Assert.ThrowsAsync<RestClientException>(() => service.GetResourceAsync());

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.ErrorResponse);
        Assert.IsType<CustomErrorMessage>(exception.ErrorResponse);
        var customError = exception.GetErrorResponse<CustomErrorMessage>();
        Assert.Equal("Invalid request parameter", customError.Message);
    }

    [Path("resource")]
    [ErrorResponse(typeof(string))]
    public interface ITestService
    {
        [Get]
        Task GetResourceAsync();
    }

    [Path("resource")]
    [ErrorResponse(typeof(CustomErrorMessage))]
    public interface ITestServiceWithCustomError
    {
        [Get]
        Task GetResourceAsync();
    }

    public record CustomErrorMessage(string Message);
}
