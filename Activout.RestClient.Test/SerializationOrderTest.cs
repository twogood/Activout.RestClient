using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.Serialization.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test;

public class SerializationOrderTest
{
    private const string BaseUri = "http://example.com/";
    private const int OrderFirst = -1000;
    private const int OrderLast = 1000;

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();


    [Theory]
    [InlineData(OrderFirst, "snake")]
    [InlineData(OrderLast, "camel")]
    public async Task TestSerializationOrder(int order, string expectedValue)
    {
        // Arrange
        var client = CreateClient(order);

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

    public class SerializationOrderModel
    {
        public string MyValue { get; set; }
    }

    public interface ISerializationOrderClient
    {
        Task<SerializationOrderModel> GetValue();
    }

    private ISerializationOrderClient CreateClient(int orderOfJsonDeserializer)
    {
        return CreateRestClientBuilder()
            .With(new JsonDeserializer(new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                })
                { Order = orderOfJsonDeserializer })
            .Build<ISerializationOrderClient>();
    }

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri));
    }
}