using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Json.Test;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Data
{
    public string? Value { get; init; }
}

public interface IClient
{
    Data GetData();
}

public class SystemTextJsonDeserializerTest
{
    private const string BaseUri = "https://example.com/api/";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();

    [Fact]
    public void TestDefaultSettingsPascalCase()
    {
        // Arrange
        _mockHttp.Expect(BaseUri)
            .Respond(new StringContent(JsonSerializer.Serialize(new
            {
                Value = "PascalCase"
            }),
                Encoding.UTF8,
                "application/json"));

        var client = CreateRestClientBuilder()
            .Build<IClient>();

        // Act
        var data = client.GetData();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal("PascalCase", data.Value);
    }

    private void MockCamelCaseResponse()
    {
        _mockHttp.Expect(BaseUri)
            .Respond(new StringContent(JsonSerializer.Serialize(new
            {
                value = "CamelCase"
            }),
                Encoding.UTF8,
                "application/json"));
    }

    [Fact]
    public void TestCamelCaseWithDeserializer()
    {
        // Arrange
        MockCamelCaseResponse();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var client = CreateRestClientBuilder()
            .With(new SystemTextJsonDeserializer(options))
            .Build<IClient>();

        // Act
        var data = client.GetData();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal("CamelCase", data.Value);
    }

    [Fact]
    public void TestCamelCaseWithSerializationManager()
    {
        // Arrange
        MockCamelCaseResponse();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var deserializers = JsonSerializationManager.DefaultDeserializers.ToList();
        deserializers.Add(new SystemTextJsonDeserializer(options));
        var serializationManager = new JsonSerializationManager(deserializers: deserializers);

        var client = CreateRestClientBuilder()
            .With(serializationManager)
            .Build<IClient>();

        // Act
        var data = client.GetData();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal("CamelCase", data.Value);
    }

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .WithSystemTextJson()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri));
    }
}

