using Activout.RestClient.Json;
using Activout.RestClient.Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Activout.RestClient.Test.Json;

public class SerializationOrderTest
{
    private const string BaseUri = "https://example.com/";
    private const int OrderFirst = -1000;
    private const int OrderLast = 1000;

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();

    [Theory]
    [InlineData(OrderFirst, "snake")]
    [InlineData(OrderLast, "camel")]
    public async Task TestSerializationOrderNewtonsoft(int order, string expectedValue)
    {
        // Arrange
        var client = CreateNewtonsoftClient(order);

        _mockHttp
            .Expect(HttpMethod.Get, BaseUri)
            .Respond(new StringContent(JsonConvert.SerializeObject(new
            {
                my_value = "snake",
                MyValue = "camel"
            }),
                Encoding.UTF8,
                "application/json"));

        // Act
        var model = await client.GetValue();

        // Assert
        Assert.Equal(expectedValue, model.MyValue);
    }

    [Theory]
    [InlineData("camel")] // System.Text.Json uses camelCase by default and matches MyValue
    public async Task TestSerializationOrderSystemTextJson(string expectedValue)
    {
        // Arrange
        var client = CreateSystemTextJsonClient();

        _mockHttp
            .Expect(HttpMethod.Get, BaseUri)
            .Respond(new StringContent(System.Text.Json.JsonSerializer.Serialize(new
            {
                myValue = "camel", // System.Text.Json maps MyValue to myValue (camelCase)
                my_value = "snake"
            }),
                Encoding.UTF8,
                "application/json"));

        // Act
        var model = await client.GetValue();

        // Assert
        Assert.Equal(expectedValue, model.MyValue);
    }

    public class SerializationOrderModel
    {
        public string MyValue { get; set; } = string.Empty;
    }

    public interface ISerializationOrderClient
    {
        Task<SerializationOrderModel> GetValue();
    }

    private ISerializationOrderClient CreateNewtonsoftClient(int orderOfJsonDeserializer)
    {
        return CreateRestClientBuilder()
            .WithNewtonsoftJson()
            .With(new NewtonsoftJsonDeserializer(new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            })
            { Order = orderOfJsonDeserializer })
            .Build<ISerializationOrderClient>();
    }

    private ISerializationOrderClient CreateSystemTextJsonClient()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return CreateRestClientBuilder()
            .WithSystemTextJson(options)
            .Build<ISerializationOrderClient>();
    }

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri));
    }
}