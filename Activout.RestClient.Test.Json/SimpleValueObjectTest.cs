using Activout.RestClient.Json;
using Activout.RestClient.Newtonsoft.Json;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;
using Xunit;

namespace Activout.RestClient.Test.Json;

public record MySimpleValueObject(string Value);

public class ApiData
{
    public MySimpleValueObject? FooBar { get; set; }
    public int? NullableInteger { get; set; }
}

public interface IValueObjectClient
{
    Task<ApiData> GetData();

    [Post]
    Task SetData(ApiData wrapper);
}

public class SimpleValueObjectTest
{
    private const string BaseUri = "https://example.com/api/";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestSimpleValueObjectSerialization(JsonImplementation jsonImplementation)
    {
        // Arrange
        _mockHttp
            .Expect(HttpMethod.Post, BaseUri)
            .WithContent("{\"FooBar\":\"foobar\",\"NullableInteger\":42}")
            .Respond(HttpStatusCode.OK);

        var client = CreateClient(jsonImplementation);

        var wrapper = new ApiData
        {
            FooBar = new MySimpleValueObject("foobar"),
            NullableInteger = 42
        };

        // Act
        await client.SetData(wrapper);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestSimpleValueObjectDeserialization(JsonImplementation jsonImplementation)
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(new StringContent("{\"FooBar\":\"foobar\",\"NullableInteger\":42}", Encoding.UTF8,
                "application/json"));

        var client = CreateClient(jsonImplementation);

        // Act
        var result = await client.GetData();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal("foobar", result.FooBar?.Value);
        Assert.Equal(42, result.NullableInteger);
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestSimpleValueObjectDeserializationWithNulls(JsonImplementation jsonImplementation)
    {
        // Arrange
        var responseContent = jsonImplementation switch
        {
            JsonImplementation.SystemTextJson => System.Text.Json.JsonSerializer.Serialize(new
            {
                FooBar = (string?)null,
                NullableInteger = (int?)null
            }),
            JsonImplementation.NewtonsoftJson => JsonConvert.SerializeObject(new
            {
                FooBar = (string?)null,
                NullableInteger = (int?)null
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonImplementation))
        };

        _mockHttp
            .Expect(BaseUri)
            .Respond(new StringContent(responseContent, Encoding.UTF8, "application/json"));

        var client = CreateClient(jsonImplementation);

        // Act
        var result = await client.GetData();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Null(result.FooBar);
        Assert.Null(result.NullableInteger);
    }

    private IValueObjectClient CreateClient(JsonImplementation jsonImplementation)
    {
        var builder = _restClientFactory.CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri));

        return jsonImplementation switch
        {
            JsonImplementation.SystemTextJson => builder.WithSystemTextJson().Build<IValueObjectClient>(),
            JsonImplementation.NewtonsoftJson => builder.WithNewtonsoftJson().Build<IValueObjectClient>(),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonImplementation))
        };
    }
}