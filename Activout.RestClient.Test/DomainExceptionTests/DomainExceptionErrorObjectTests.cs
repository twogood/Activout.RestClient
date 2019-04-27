using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.DomainErrors;
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

        public MyDomainErrorObjectException(MyDomainErrorObject error)
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

    internal class MyDomainErrorObjectMapper : AbstractDomainErrorMapper
    {
        protected override object Map(HttpResponseMessage httpResponseMessage, object data)
        {
            if (!(data is MyApiErrorResponse errorResponse)) return null;

            return errorResponse.Code == MyApiError.Bar
                ? new MyDomainErrorObject(MyDomainErrorEnum.DomainBar)
                : new MyDomainErrorObject(MyDomainErrorEnum.Unknown);
        }
    }

    internal class MyDomainErrorMapperFactory : DefaultDomainErrorMapperFactory
    {
        public override IDomainErrorMapper CreateDomainErrorMapper(Type errorResponseType, Type domainErrorType,
            IEnumerable<DomainHttpErrorAttribute> httpErrorAttributes)
        {
            return domainErrorType == typeof(MyDomainErrorObject)
                ? new MyDomainErrorObjectMapper()
                : base.CreateDomainErrorMapper(errorResponseType, domainErrorType, httpErrorAttributes);
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
                .HttpClient(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri))
                .With(new MyDomainErrorMapperFactory())
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