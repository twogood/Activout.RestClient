using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.RestClient.Test.MovieReviews;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test;

public class DictionaryParameterTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/api";
    
    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new MockHttpMessageHandler();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_loggerFactory.CreateLogger<DictionaryParameterTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri);
    }

    [Fact]
    public async Task TestQueryParamDictionary()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var queryParams = new Dictionary<string, string>
        {
            ["param1"] = "value1",
            ["param2"] = "value2"
        };

        _mockHttp
            .When("https://example.com/test")
            .WithQueryString("param1=value1&param2=value2")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamDictionary(queryParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestFormParamDictionary()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var formParams = new Dictionary<string, string>
        {
            ["field1"] = "value1",
            ["field2"] = "value2"
        };

        _mockHttp
            .When(HttpMethod.Post, "https://example.com/test")
            .WithFormData("field1", "value1")
            .WithFormData("field2", "value2")
            .Respond("application/json", "{}");

        // act
        await service.TestFormParamDictionary(formParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestHeaderParamDictionary()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var headers = new Dictionary<string, string>
        {
            ["X-Custom-Header1"] = "value1",
            ["X-Custom-Header2"] = "value2"
        };

        _mockHttp
            .When("https://example.com/test")
            .WithHeaders("X-Custom-Header1", "value1")
            .WithHeaders("X-Custom-Header2", "value2")
            .Respond("application/json", "{}");

        // act
        await service.TestHeaderParamDictionary(headers);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestMixedDictionaryAndRegularParams()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var queryParams = new Dictionary<string, string>
        {
            ["param1"] = "value1",
            ["param2"] = "value2"
        };

        _mockHttp
            .When("https://example.com/test")
            .WithQueryString("param1=value1&param2=value2&singleParam=singleValue")
            .Respond("application/json", "{}");

        // act
        await service.TestMixedParams(queryParams, "singleValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestEmptyDictionary()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var emptyParams = new Dictionary<string, string>();

        _mockHttp
            .When("https://example.com/test")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamDictionary(emptyParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestNullDictionaryValues()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var paramsWithNull = new Dictionary<string, string>
        {
            ["param1"] = "value1",
            ["param2"] = null
        };

        _mockHttp
            .When("https://example.com/test")
            .WithQueryString("param1=value1&param2=")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamDictionary(paramsWithNull);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestBackwardCompatibilityWithNonDictionaryParams()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();

        _mockHttp
            .When("https://example.com/test")
            .WithQueryString("regularParam=regularValue")
            .Respond("application/json", "{}");

        // act
        await service.TestRegularParam("regularValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}

public interface ITestService
{
    [Get("/test")]
    Task TestQueryParamDictionary([QueryParam] Dictionary<string, string> queryParams);

    [Post("/test")]
    Task TestFormParamDictionary([FormParam] Dictionary<string, string> formParams);

    [Get("/test")]
    Task TestHeaderParamDictionary([HeaderParam] Dictionary<string, string> headers);

    [Get("/test")]
    Task TestMixedParams([QueryParam] Dictionary<string, string> queryParams, [QueryParam("singleParam")] string singleParam);

    [Get("/test")]
    Task TestRegularParam([QueryParam("regularParam")] string regularParam);
}