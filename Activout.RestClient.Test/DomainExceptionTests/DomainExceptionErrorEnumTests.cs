using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.DomainExceptions;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal class MyDomainErrorEnumException : Exception
    {
        public MyDomainErrorEnum Error { get; }

        public MyDomainErrorEnumException(MyDomainErrorEnum error)
        {
            Error = error;
        }
    }

    [ErrorResponse(typeof(MyApiErrorResponse))]
    [DomainException(typeof(MyDomainErrorEnumException))]
    [MyDomainHttpError(HttpStatusCode.InternalServerError, MyDomainErrorEnum.ServerError)]
    public interface IMyApiClient
    {
        [MyDomainHttpError(HttpStatusCode.Forbidden, MyDomainErrorEnum.AccessDenied)]
        Task ForbiddenToAccessDenied();

        [MyDomainHttpError(HttpStatusCode.BadRequest, MyDomainErrorEnum.ClientError)]
        Task Api();
    }


    public class DomainExceptionErrorEnumTests
    {
        private const string BaseUri = "https://example.com";

        private readonly MockHttpMessageHandler _mockHttp;
        private readonly IMyApiClient _myApiClient;

        public DomainExceptionErrorEnumTests()
        {
            _mockHttp = new MockHttpMessageHandler();

            _myApiClient = Services.CreateRestClientFactory()
                .CreateBuilder()
                .HttpClient(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri))
                .Build<IMyApiClient>();
        }

        [Fact]
        public async Task TestForbiddenToAccessDenied()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(HttpStatusCode.Forbidden);

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.ForbiddenToAccessDenied());

            // Assert
            Assert.Equal(MyDomainErrorEnum.AccessDenied, exception.Error);
        }

        [Fact]
        public async Task TestAnyServerError()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(HttpStatusCode.ServiceUnavailable);

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.ServerError, exception.Error);
        }

        [Fact]
        public async Task TestAnyClientError()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(HttpStatusCode.MethodNotAllowed);

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.ClientError, exception.Error);
        }


        [Fact]
        public async Task TestForbidden()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(HttpStatusCode.Forbidden);

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.Forbidden, exception.Error);
        }

        [Fact]
        public async Task TestMapApiErrorCode()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(_ => JsonHttpResponseMessage(HttpStatusCode.BadRequest, MyApiError.Foo));

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.DomainFoo, exception.Error);
        }

        [Fact]
        public async Task TestBadRequestToClientError()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(_ => JsonHttpResponseMessage(HttpStatusCode.BadRequest, MyApiError.Bar));

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorEnumException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.ClientError, exception.Error);
        }

        private static HttpResponseMessage JsonHttpResponseMessage(HttpStatusCode httpStatusCode, MyApiError myApiError)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new MyApiErrorResponse
                {
                    Code = myApiError
                }), Encoding.UTF8, "application/json")
            };
        }
    }
}