using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    public class HttpResponseMessageTest
    {
        private const string BaseUri = "https://example.com/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        public HttpResponseMessageTest()
        {
            _restClientFactory = new RestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task TestHttpResponseMessage(HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Get, BaseUri + "response-message")
                .Respond(expectedStatusCode);

            // Act
            var httpResponseMessage = await client.GetHttpResponseMessage();

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.Equal(expectedStatusCode, httpResponseMessage.StatusCode);
        }

        [Path("response-message")]
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IHttpResponseMessageClient
        {
            public Task<HttpResponseMessage> GetHttpResponseMessage();
        }

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }

        private IHttpResponseMessageClient CreateClient()
        {
            return CreateRestClientBuilder()
                .Build<IHttpResponseMessageClient>();
        }
    }
}