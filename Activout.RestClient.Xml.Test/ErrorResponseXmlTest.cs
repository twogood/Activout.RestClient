using System.Net;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit.Abstractions;

namespace Activout.RestClient.Xml.Test;

public class ErrorResponseXmlTest(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/api";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private ITestService CreateTestService()
    {
        return _restClientFactory.CreateBuilder()
            .WithXml()
            .With(_loggerFactory.CreateLogger<ErrorResponseXmlTest>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<ITestService>();
    }

    [Fact]
    public async Task TestErrorResponse_Xml_BadRequest()
    {
        // arrange
        const string errorXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <error>
                <code>400</code>
                <message>Invalid request parameter</message>
            </error>
            """;
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/resource")
            .Respond(HttpStatusCode.BadRequest, "text/xml", errorXml);

        var service = CreateTestService();

        // act
        var exception = await Assert.ThrowsAsync<RestClientException>(() => service.GetResource());

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.NotNull(exception.ErrorResponse);
        Assert.IsType<XmlErrorResponse>(exception.ErrorResponse);
        var errorResponse = exception.GetErrorResponse<XmlErrorResponse>();
        Assert.Equal(400, errorResponse.Code);
        Assert.Equal("Invalid request parameter", errorResponse.Message);
    }

    [Path("resource")]
    [ErrorResponse(typeof(XmlErrorResponse))]
    public interface ITestService
    {
        [Get]
        Task GetResource();
    }

    [XmlRoot("error")]
    public class XmlErrorResponse
    {
        [XmlElement("code")]
        public int Code { get; set; }

        [XmlElement("message")]
        public string Message { get; set; } = "";
    }
}
