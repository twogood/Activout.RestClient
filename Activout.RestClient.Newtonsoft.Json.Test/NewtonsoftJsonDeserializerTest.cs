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

namespace Activout.RestClient.Newtonsoft.Json.Test
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Data
    {
        public string? Value { get; init; }
    }

    public interface IClient
    {
        Data GetData();
    }

    public class NewtonsoftJsonDeserializerTest
    {
        public NewtonsoftJsonDeserializerTest()
        {
            _restClientFactory = new RestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        private const string BaseUri = "https://example.com/api/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        [Fact]
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
                .With(new NewtonsoftJsonDeserializer(new JsonSerializerSettings
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
        public void TestCamelCaseSerializerSettings()
        {
            // Arrange
            MockCamelCaseResponse();

            var client = CreateRestClientBuilder(NewtonsoftJsonDefaults.CamelCaseSerializerSettings)
                .Build<IClient>();

            // Act
            var data = client.GetData();

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.Equal("CamelCase", data.Value);
        }

        private IRestClientBuilder CreateRestClientBuilder(JsonSerializerSettings? jsonSerializerSettings = null)
        {
            return _restClientFactory.CreateBuilder()
                .WithNewtonsoftJson(jsonSerializerSettings)
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }
    }
}