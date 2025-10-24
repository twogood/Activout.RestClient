#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test;

public class DictionaryParameterTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/api";
    
    private readonly RestClientFactory _restClientFactory = new RestClientFactory();
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
        var queryParams = new Dictionary<string, string?>
        {
            ["param1"] = "value1",
            ["param2"] = "value2"
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString("param1=value1&param2=value2")
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
            .When(HttpMethod.Post, "https://example.com/api/test")
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
            .When("https://example.com/api/test")
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
            .When("https://example.com/api/test")
            .WithExactQueryString("param1=value1&param2=value2&singleParam=singleValue")
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
        var emptyParams = new Dictionary<string, string?>();

        _mockHttp
            .When("https://example.com/api/test")
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
        var paramsWithNull = new Dictionary<string, string?>
        {
            ["param1"] = "value1",
            ["param2"] = null
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString("param1=value1")
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
            .When("https://example.com/api/test")
            .WithExactQueryString("regularParam=regularValue")
            .Respond("application/json", "{}");

        // act
        await service.TestRegularParam("regularValue");

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestQueryParamDictionaryWithDateTime()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var testDate = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);
        var expectedDateString = testDate.ToString("o"); // ISO 8601 format
        var queryParams = new Dictionary<string, object>
        {
            ["stringParam"] = "value1",
            ["dateParam"] = testDate
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString($"stringParam=value1&dateParam={Uri.EscapeDataString(expectedDateString)}")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamObjectDictionary(queryParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestFormParamDictionaryWithDateTime()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var testDate = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);
        var expectedDateString = testDate.ToString("o"); // ISO 8601 format
        var formParams = new Dictionary<string, object>
        {
            ["stringField"] = "value1",
            ["dateField"] = testDate
        };

        _mockHttp
            .When(HttpMethod.Post, "https://example.com/api/test")
            .WithFormData("stringField", "value1")
            .WithFormData("dateField", expectedDateString)
            .Respond("application/json", "{}");

        // act
        await service.TestFormParamObjectDictionary(formParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestHeaderParamDictionaryWithDateTime()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var testDate = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);
        var expectedDateString = testDate.ToString("o"); // ISO 8601 format
        var headers = new Dictionary<string, object>
        {
            ["X-String-Header"] = "value1",
            ["X-Date-Header"] = testDate
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithHeaders("X-String-Header", "value1")
            .WithHeaders("X-Date-Header", expectedDateString)
            .Respond("application/json", "{}");

        // act
        await service.TestHeaderParamObjectDictionary(headers);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestGenericDictionaryWithDateTime()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var testDate = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);
        var expectedDateString = testDate.ToString("o"); // ISO 8601 format
        var queryParams = new Dictionary<string, DateTime>
        {
            ["startDate"] = testDate,
            ["endDate"] = testDate.AddDays(1)
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString($"startDate={Uri.EscapeDataString(expectedDateString)}&endDate={Uri.EscapeDataString(testDate.AddDays(1).ToString("o"))}")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamDateTimeDictionary(queryParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestNonGenericDictionaryWithDateTime()
    {
        // arrange
        var service = CreateRestClientBuilder().Build<ITestService>();
        var testDate = new DateTime(2023, 12, 25, 14, 30, 45, DateTimeKind.Utc);
        var expectedDateString = testDate.ToString("o"); // ISO 8601 format
        
        var queryParams = new Hashtable
        {
            ["stringParam"] = "value1",
            ["dateParam"] = testDate
        };

        _mockHttp
            .When("https://example.com/api/test")
            .WithExactQueryString($"stringParam=value1&dateParam={Uri.EscapeDataString(expectedDateString)}")
            .Respond("application/json", "{}");

        // act
        await service.TestQueryParamNonGenericDictionary(queryParams);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}

public interface ITestService
{
    [Get("test")]
    Task TestQueryParamDictionary([QueryParam] Dictionary<string, string?> queryParams);

    [Post("test")]
    Task TestFormParamDictionary([FormParam] Dictionary<string, string> formParams);

    [Get("test")]
    Task TestHeaderParamDictionary([HeaderParam] Dictionary<string, string> headers);

    [Get("test")]
    Task TestMixedParams([QueryParam] Dictionary<string, string> queryParams, [QueryParam("singleParam")] string singleParam);

    [Get("test")]
    Task TestRegularParam([QueryParam("regularParam")] string regularParam);

    [Get("test")]
    Task TestQueryParamObjectDictionary([QueryParam] Dictionary<string, object> queryParams);

    [Post("test")]
    Task TestFormParamObjectDictionary([FormParam] Dictionary<string, object> formParams);

    [Get("test")]
    Task TestHeaderParamObjectDictionary([HeaderParam] Dictionary<string, object> headers);

    [Get("test")]
    Task TestQueryParamDateTimeDictionary([QueryParam] Dictionary<string, DateTime> queryParams);

    [Get("test")]
    Task TestQueryParamNonGenericDictionary([QueryParam] IDictionary queryParams);
}