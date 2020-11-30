using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    public class HttpStatusCodeTest
    {
        private const string BaseUri = "http://example.com/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        public HttpStatusCodeTest()
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task TestStatusCodeResponse(HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Get, BaseUri + "status-code")
                .Respond(expectedStatusCode);

            // Act
            var statusCode = await client.GetStatusCode();

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.Equal(expectedStatusCode, statusCode);
        }

        [Path("status-code")]
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IHttpStatusCodeClient
        {
            public Task<HttpStatusCode> GetStatusCode();
        }

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }

        private IHttpStatusCodeClient CreateClient()
        {
            return CreateRestClientBuilder()
                .Build<IHttpStatusCodeClient>();
        }
    }
}