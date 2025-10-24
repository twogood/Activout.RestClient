using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test;

public class NullParameterTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/api";
    
    private readonly IRestClientFactory _restClientFactory = new RestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new MockHttpMessageHandler();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_loggerFactory.CreateLogger<NullParameterTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri);
    }

    [Fact]
    public async Task TestEmptyStringQueryParam_ShouldAddParameter()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: empty string should still be added as parameter
        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString("param=")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParam("");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestEmptyStringFormParam_ShouldAddParameter()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: empty string should still be added as form parameter
        _mockHttp
            .When(HttpMethod.Post, "https://example.com/api/test")
            .WithExactFormData([
                new KeyValuePair<string, string>("param", "")
            ])
            .Respond("application/json", "{}");

        // act
        await service.TestFormParam("");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestEmptyStringHeaderParam_ShouldAddHeader()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: empty string should still be added as header
        _mockHttp
            .When("https://example.com/api/test")
            .WithHeaders("X-Custom-Header", "")
            .Respond("application/json", "{}");

        // act
        await service.TestHeaderParam("");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestMixedNullAndValidQueryParams_ShouldOnlyAddValidParams()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: only the valid parameter should be added, null param should be skipped
        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString("validParam=validValue")
            .Respond("application/json", "{}");

        // act
        await service.TestMixedQueryParams(null, "validValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestMixedNullAndValidFormParams_ShouldOnlyAddValidParams()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: only the valid form parameter should be added, null param should be skipped
        _mockHttp
            .When(HttpMethod.Post, "https://example.com/api/test")
            .WithExactFormData([
                new KeyValuePair<string, string>("validParam", "validValue")
            ])
            .Respond("application/json", "{}");

        // act
        await service.TestMixedFormParams(null, "validValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestMixedNullAndValidHeaderParams_ShouldOnlyAddValidParams()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();

        // expect: only the valid header should be added, null param should be skipped
        _mockHttp
            .When("https://example.com/api/test")
            .WithHeaders("X-Valid-Header", "validValue")
            .With(req => !req.Headers.Contains("X-Null-Header"))
            .Respond("application/json", "{}");

        // act
        await service.TestMixedHeaderParams(null, "validValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestNullValueInQueryDictionary_SkipsNullValues()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();
        var queryParams = new Dictionary<string, string?>
        {
            ["param1"] = "value1",
            ["param2"] = null,  // This should be skipped (already working correctly)
            ["param3"] = "value3"
        };

        // Dictionary handling already works correctly - only non-null values are added
        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString("param1=value1&param3=value3")
            .Respond("application/json", "{}");

        // act
        await service.TestNullDictionaryValues(queryParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestNullValueInFormDictionary_SkipsNullValues()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();
        var formParams = new Dictionary<string, string?>
        {
            ["field1"] = "value1",
            ["field2"] = null,  // This should be skipped (already working correctly)
            ["field3"] = "value3"
        };

        // Dictionary handling already works correctly - only non-null values are added
        _mockHttp
            .When(HttpMethod.Post, "https://example.com/api/test")
            .WithExactFormData([
                new KeyValuePair<string, string>("field1", "value1"),
                new KeyValuePair<string, string>("field3", "value3")
            ])
            .Respond("application/json", "{}");

        // act
        await service.TestNullFormDictionaryValues(formParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestNullValueInHeaderDictionary_SkipsNullValues()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestNullService>();
        var headers = new Dictionary<string, string?>
        {
            ["X-Header-1"] = "value1",
            ["X-Header-2"] = null,  // This should be skipped (already working correctly)
            ["X-Header-3"] = "value3"
        };

        // Dictionary handling already works correctly - only non-null values are added
        _mockHttp
            .When("https://example.com/api/test")
            .WithHeaders("X-Header-1", "value1")
            .WithHeaders("X-Header-3", "value3")
            .With(req => !req.Headers.Contains("X-Header-2"))
            .Respond("application/json", "{}");

        // act
        await service.TestNullHeaderDictionaryValues(headers);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}

public interface ITestNullService
{
    [Get("test")]
    Task TestQueryParam([QueryParam("param")] string param);

    [Post("test")]
    Task TestFormParam([FormParam("param")] string param);

    [Get("test")]
    Task TestHeaderParam([HeaderParam("X-Custom-Header")] string param);

    [Get("test")]
    Task TestMixedQueryParams([QueryParam("nullParam")] string? nullParam, [QueryParam("validParam")] string validParam);

    [Post("test")]
    Task TestMixedFormParams([FormParam("nullParam")] string? nullParam, [FormParam("validParam")] string validParam);

    [Get("test")]
    Task TestMixedHeaderParams([HeaderParam("X-Null-Header")] string? nullHeader, [HeaderParam("X-Valid-Header")] string validHeader);

    [Get("test")]
    Task TestNullDictionaryValues([QueryParam] Dictionary<string, string?> queryParams);

    [Post("test")]
    Task TestNullFormDictionaryValues([FormParam] Dictionary<string, string?> formParams);

    [Get("test")]
    Task TestNullHeaderDictionaryValues([HeaderParam] Dictionary<string, string?> headers);
}