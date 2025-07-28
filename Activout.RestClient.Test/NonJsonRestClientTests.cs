using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.Test.MovieReviews;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test
{
    public class NonJsonRestClientTests
    {
        public NonJsonRestClientTests(ITestOutputHelper outputHelper)
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
            _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);
        }

        private const string BaseUri = "https://example.com/movieReviewService";
        private const string MovieId = "*MOVIE_ID*";
        private const string ReviewId = "*REVIEW_ID*";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly ILoggerFactory _loggerFactory;

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .With(_loggerFactory.CreateLogger<NonJsonRestClientTests>())
                .With(_mockHttp.ToHttpClient())
                .BaseUri(BaseUri);
        }

        private IMovieReviewService CreateMovieReviewService()
        {
            return CreateRestClientBuilder()
                .Build<IMovieReviewService>();
        }

        [Fact]
        public void TestErrorEmptyNoContentType()
        {
            // arrange
            _mockHttp
                .When(HttpMethod.Get, $"{BaseUri}/movies/fail")
                .Respond(HttpStatusCode.BadRequest, request => new ByteArrayContent(new byte[0]));

            var reviewSvc = CreateMovieReviewService();

            // act
            var aggregateException = Assert.Throws<AggregateException>(() => reviewSvc.Fail());

            // assert
            var exception = (RestClientException)aggregateException.GetBaseException();
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            Assert.NotNull(exception.ErrorResponse);
            Assert.IsType<byte[]>(exception.ErrorResponse);
            Assert.Empty(exception.GetErrorResponse<byte[]>());
        }

        [Fact]
        public void TestDelete()
        {
            // arrange
            _mockHttp
                .Expect(HttpMethod.Delete, $"{BaseUri}/movies/{MovieId}/reviews/{ReviewId}")
                .Respond(HttpStatusCode.OK);

            var reviewSvc = CreateMovieReviewService();

            // act
            reviewSvc.DeleteReview(MovieId, ReviewId);

            // assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestPostTextAsync()
        {
            // arrange
            _mockHttp
                .When(HttpMethod.Post, $"{BaseUri}/movies/import.csv")
                .WithContent("foobar")
                .WithHeaders("Content-Type", "text/csv; charset=utf-8")
                .Respond(HttpStatusCode.NoContent);

            var reviewSvc = CreateMovieReviewService();

            // act
            await reviewSvc.Import("foobar");

            // assert
            //Assert.Equal("*REVIEW_ID*", result.ReviewId);
        }

        [Fact]
        public async Task TestGetHttpContent()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond("text/plain", "test content");

            var reviewSvc = CreateMovieReviewService();

            // act
            var httpContent = reviewSvc.GetHttpContent();

            // assert
            Assert.Equal("test content", await httpContent.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestGetHttpResponseMessage()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond("text/plain", "test content");

            var reviewSvc = CreateMovieReviewService();

            // act
            var httpResponseMessage = reviewSvc.GetHttpResponseMessage();

            // assert
            Assert.Equal("test content", await httpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestGetByteArray()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/bytes")
                .Respond(new ByteArrayContent(new byte[] { 42 }));

            var reviewSvc = CreateMovieReviewService();

            // act
            var bytes = await reviewSvc.GetByteArray();

            // assert
            Assert.Equal(new byte[] { 42 }, bytes);
        }

        [Fact]
        public async Task TestGetByteArrayObject()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/byte-object")
                .Respond(new ByteArrayContent(new byte[] { 42 }));

            var reviewSvc = CreateMovieReviewService();

            // act
            var byteArrayObject = await reviewSvc.GetByteArrayObject();

            // assert
            Assert.Equal(new byte[] { 42 }, byteArrayObject.Bytes);
        }

        [Fact]
        public async Task TestGetString()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/string")
                .WithHeaders("accept", "text/plain")
                .Respond(new StringContent("foo"));

            var reviewSvc = CreateMovieReviewService();

            // act
            var result = await reviewSvc.GetString();

            // assert
            Assert.Equal("foo", result);
        }

        [Fact]
        public async Task TestGetStringObject()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/string-object")
                .WithHeaders("accept", "text/plain")
                .Respond(new StringContent("bar"));

            var reviewSvc = CreateMovieReviewService();

            // act
            var stringObject = await reviewSvc.GetStringObject();

            // assert
            Assert.Equal("bar", stringObject.Value);
        }

        [Fact]
        public async Task TestFormPost()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/form")
                .WithFormData("value", "foobar")
                .Respond("text/plain", "");

            var reviewSvc = CreateMovieReviewService();

            // act
            await reviewSvc.FormPost("foobar");

            // assert
        }

        [Fact]
        public async Task TestHeaderParam()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/headers")
                .WithHeaders("X-Foo", "bar")
                .Respond("text/plain", "");

            var reviewSvc = CreateRestClientBuilder()
                .Header(new AuthenticationHeaderValue("Basic", "SECRET"))
                .Header("X-Tick", new TickValue())
                .Build<IMovieReviewService>();

            // act
            var responseMessage1 = await reviewSvc.SendFooHeader("bar");
            var requestHeaders1 = responseMessage1.RequestMessage.Headers;

            var responseMessage2 = await reviewSvc.SendFooHeader("bar");
            var requestHeaders2 = responseMessage2.RequestMessage.Headers;

            // assert
            Assert.NotNull(requestHeaders1.Authorization);
            Assert.Equal("Basic SECRET", requestHeaders1.Authorization.ToString());
            Assert.NotEmpty(requestHeaders1.GetValues("X-Tick"));

            Assert.NotEqual(
                requestHeaders1.GetValues("X-Tick").First(),
                requestHeaders2.GetValues("X-Tick").First());
        }
    }

    internal class TickValue
    {
        private long _ticks = 42;

        public override string ToString()
        {
            return Interlocked.Increment(ref _ticks).ToString();
        }
    }
}