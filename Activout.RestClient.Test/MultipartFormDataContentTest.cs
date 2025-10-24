#nullable disable
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test
{
    public class MultipartFormDataContentTest
    {
        private const string BaseUri = "https://example.com/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly ILoggerFactory _loggerFactory;

        public MultipartFormDataContentTest(ITestOutputHelper outputHelper)
        {
            _restClientFactory = new RestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
            _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);
        }

        [Fact]
        public async Task TestSendNullPart()
        {
            // Arrange
            var client = CreateClient();
            var collector = new HttpRequestMessageCollector();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "multipart")
                .With(message =>
                {
                    collector.Message = message;
                    return message.Content?.Headers.ContentType?.MediaType == "multipart/form-data";
                })
                .Respond(HttpStatusCode.OK);

            // Act
            await client.SendParts(null, 42);

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();

            var multipartFormDataContent = collector.Message?.Content as MultipartFormDataContent;
            Assert.NotNull(multipartFormDataContent);

            var content = multipartFormDataContent.ToArray();
            Assert.Single(content);

            var part = content[0];
            Assert.Equal("42", await part.ReadAsStringAsync());
            Assert.Equal("bar", part.Headers.ContentDisposition?.Name);
            Assert.Equal("form-data", part.Headers.ContentDisposition?.DispositionType);
        }

        [Fact]
        public async Task TestSendFormInForm()
        {
            // Arrange
            var client = CreateClient();
            var collector = new HttpRequestMessageCollector();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "multipart")
                .With(message =>
                {
                    collector.Message = message;
                    return message.Content?.Headers.ContentType?.MediaType == "multipart/form-data";
                })
                .Respond(HttpStatusCode.OK);

            // Act
            await client.SendFormInForm(new FormModel
            {
                MyInt = 42,
                MyString = "foobar"
            }, new[]
            {
                new Part(Content: "foo", Name: "foo", FileName: "foo.txt"),
                new Part(Content: "bar", Name: "bar", FileName: "bar.txt")
            });

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();

            var multipartFormDataContent = collector.Message?.Content as MultipartFormDataContent;
            Assert.NotNull(multipartFormDataContent);

            var content = multipartFormDataContent.ToArray();
            Assert.Equal(3, content.Length);

            var formContent = content[0];
            Assert.Equal("MyString=foobar&MyInt=42", await formContent.ReadAsStringAsync());
            Assert.Null(formContent.Headers.ContentDisposition?.Name);
            Assert.Null(formContent.Headers.ContentDisposition?.FileName);

            var attachment1 = content[1];
            Assert.Equal("foo", await attachment1.ReadAsStringAsync());
            Assert.Equal("attachment", attachment1.Headers.ContentDisposition?.Name);
            Assert.Equal("foo.txt", attachment1.Headers.ContentDisposition?.FileName);

            var attachment2 = content[2];
            Assert.Equal("bar", await attachment2.ReadAsStringAsync());
            Assert.Equal("attachment", attachment2.Headers.ContentDisposition?.Name);
            Assert.Equal("bar.txt", attachment2.Headers.ContentDisposition?.FileName);
        }

        [Fact]
        public async Task TestSendMultipartFormDataContent()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "multipart")
                .With(message => message.Content?.Headers.ContentType?.MediaType == "multipart/form-data")
                .Respond(HttpStatusCode.OK);

            // Act
            var content = new MultipartFormDataContent();
            await client.SendMultipartFormDataContent(content);

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestReceiveMultipartFormDataContent()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Get, BaseUri + "multipart")
                .Respond(HttpStatusCode.OK, new MultipartFormDataContent());

            // Act
            await client.ReceiveMultipartFormDataContent();

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

            [Post]
            Task SendFormInForm(
                [PartParam("", contentType: "application/x-www-form-urlencoded")]
                FormModel form,
                [PartParam("attachment", contentType: "application/octet-stream")]
                Part[] parts);


            [Post]
            Task SendParts(
                [PartParam] string? foo,
                [PartParam] int bar);
        }

        public class FormModel
        {
            public string? MyString { get; set; }
            public int? MyInt { get; set; }
        }

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .With(_mockHttp.ToHttpClient())
                .With(_loggerFactory.CreateLogger<MultipartFormDataContentTest>())
                .BaseUri(new Uri(BaseUri));
        }

        private IMultipartFormDataContentClient CreateClient()
        {
            return CreateRestClientBuilder()
                .Build<IMultipartFormDataContentClient>();
        }
    }
}

