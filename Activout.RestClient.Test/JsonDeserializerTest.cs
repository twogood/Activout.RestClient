using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using Activout.RestClient.Serialization.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Data
    {
        public string Value { get; set; }
    }

    public interface IClient
    {
        Data GetData();
    }

    public class JsonDeserializerTest
    {
        public JsonDeserializerTest()
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        private const string BaseUri = "http://example.com/api/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        [Fact(Skip = "Test if CI works")]
        public void TestDefaultSettingsPascalCase()
        {
            // Arrange
            _mockHttp.Expect(BaseUri)
                .Respond(new StringContent(JsonConvert.SerializeObject(new
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
                .Respond(new StringContent(JsonConvert.SerializeObject(new
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

            var client = CreateRestClientBuilder()
                .With(new JsonDeserializer(new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }))
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

            var deserializers = SerializationManager.DefaultDeserializers.ToList();
            deserializers.Add(new JsonDeserializer(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
            var serializationManager = new SerializationManager(deserializers: deserializers);

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
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }
    }
}