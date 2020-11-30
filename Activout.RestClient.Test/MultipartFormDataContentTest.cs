using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.RestClient.Serialization.Implementation;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    public class MultipartFormDataContentTest
    {
        private const string BaseUri = "http://example.com/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        public MultipartFormDataContentTest()
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        [Fact]
        public async Task TestSendMultipart()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "multipart")
                .With(message => message.Content.Headers.ContentType.MediaType == "multipart/form-data")
                .Respond(HttpStatusCode.OK);

            // Act
            var content = new MultipartFormDataContent();
            await client.SendMultipartFormDataContent(content);

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestReceiveMultipart()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Get, BaseUri + "multipart")
                .Respond(HttpStatusCode.OK, new MultipartFormDataContent());

            // Act
            var content = await client.ReceiveMultipartFormDataContent();

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestReceiveMultipartAsHttpContent()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Get, BaseUri + "multipart")
                .Respond(HttpStatusCode.OK, new MultipartFormDataContent());

            // Act
            var content = await client.ReceiveHttpContent();

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
            Assert.IsType<MultipartFormDataContent>(content);
        }


        [Path("multipart")]
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IMultipartFormDataContentClient
        {
            [Post]
            public Task<HttpResponseMessage> SendMultipartFormDataContent(MultipartFormDataContent content);

            [Accept("multipart/form-data")]
            public Task<MultipartFormDataContent> ReceiveMultipartFormDataContent();

            [Accept("multipart/form-data")]
            public Task<HttpContent> ReceiveHttpContent();
        }

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }

        private IMultipartFormDataContentClient CreateClient()
        {
            return CreateRestClientBuilder()
                .Build<IMultipartFormDataContentClient>();
        }
    }
}