using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.DomainExceptions;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test.DomainExceptionTests
{
    internal class MyDomainErrorObject
    {
        public MyDomainErrorEnum ErrorEnum { get; }

        public MyDomainErrorObject(MyDomainErrorEnum errorEnum)
        {
            ErrorEnum = errorEnum;
        }
    }

    internal class MyDomainErrorObjectException : Exception
    {
        public MyDomainErrorObject Error { get; }

        public MyDomainErrorObjectException(MyDomainErrorObject error, Exception innerException = null) : base(
            error.ToString(), innerException)
        {
            Error = error;
        }
    }

    [ErrorResponse(typeof(MyApiErrorResponse))]
    [DomainException(typeof(MyDomainErrorObjectException))]
    public interface IMyApiErrorObjectClient
    {
        Task Api();
    }

    internal class MyDomainExceptionObjectMapper : AbstractDomainExceptionMapper
    {
        protected override Exception CreateException(HttpResponseMessage httpResponseMessage, object data)
        {
            if (data is Exception e)
            {
                return new MyDomainErrorObjectException(new MyDomainErrorObject(MyDomainErrorEnum.Unknown), e);
            }

            var domainError = data is MyApiErrorResponse errorResponse && errorResponse.Code == MyApiError.Bar
                ? new MyDomainErrorObject(MyDomainErrorEnum.DomainBar)
                : new MyDomainErrorObject(MyDomainErrorEnum.Unknown);

            return new MyDomainErrorObjectException(domainError);
        }
    }

    internal class MyDomainExceptionMapperFactory : DefaultDomainExceptionMapperFactory
    {
        public override IDomainExceptionMapper CreateDomainExceptionMapper(
            MethodInfo method,
            Type errorResponseType,
            Type exceptionType)
        {
            return exceptionType == typeof(MyDomainErrorObjectException)
                ? new MyDomainExceptionObjectMapper()
                : base.CreateDomainExceptionMapper(method, errorResponseType, exceptionType);
        }
    }

    public class DomainExceptionErrorObjectTests
    {
        private const string BaseUri = "https://example.com";

        private readonly MockHttpMessageHandler _mockHttp;
        private readonly IMyApiErrorObjectClient _myApiClient;

        public DomainExceptionErrorObjectTests()
        {
            _mockHttp = new MockHttpMessageHandler();

            _myApiClient = Services.CreateRestClientFactory()
                .CreateBuilder()
                .With(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri))
                .With(new MyDomainExceptionMapperFactory())
                .Build<IMyApiErrorObjectClient>();
        }

        [Fact]
        public async Task TestMapApiErrorObject()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(_ => JsonHttpResponseMessage(HttpStatusCode.BadRequest, MyApiError.Bar));

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorObjectException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.DomainBar, exception.Error.ErrorEnum);
        }

        [Fact]
        public async Task TestNoDeserializerFound()
        {
            // Arrange
            _mockHttp
                .Expect(BaseUri)
                .Respond(_ => HtmlHttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var exception = await Assert.ThrowsAsync<MyDomainErrorObjectException>(() =>
                _myApiClient.Api());

            // Assert
            Assert.Equal(MyDomainErrorEnum.Unknown, exception.Error.ErrorEnum);
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

        private static HttpResponseMessage HtmlHttpResponseMessage(HttpStatusCode httpStatusCode)
        {
            return new HttpResponseMessage(httpStatusCode)
            {
                Content = new StringContent($"<html><head><title>Error {httpStatusCode}</title></head></html>",
                    Encoding.UTF8, "text/html")
            };
        }
    }
}