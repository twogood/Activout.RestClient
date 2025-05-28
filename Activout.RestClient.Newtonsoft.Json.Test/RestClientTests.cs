using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Newtonsoft.Json.Test.MovieReviews;
using Activout.RestClient.Test.MovieReviews;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Newtonsoft.Json.Test
{
    public class RestClientTests
    {
        public RestClientTests(ITestOutputHelper outputHelper)
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
                .WithNewtonsoftJson()
                .Accept("application/json")
                .ContentType("application/json")
                .With(_loggerFactory.CreateLogger<RestClientTests>())
                .With(_mockHttp.ToHttpClient())
                .BaseUri(BaseUri);
        }

        private IMovieReviewService CreateMovieReviewService()
        {
            return CreateRestClientBuilder()
                .Build<IMovieReviewService>();
        }

        [Fact]
        public async Task TestErrorAsyncWithOldTaskConverter()
        {
            // arrange
            ExpectGetAllReviewsAndReturnError();

            var reviewSvc = CreateRestClientBuilder()
                .With(new TaskConverterFactory())
                .Build<IMovieReviewService>();

            // act
            var aggregateException =
                await Assert.ThrowsAsync<AggregateException>(() => reviewSvc.GetAllReviews(MovieId));

            // assert
            _mockHttp.VerifyNoOutstandingExpectation();

            Assert.IsType<RestClientException>(aggregateException.InnerException);
            var exception = (RestClientException)aggregateException.InnerException;

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            var error = exception.GetErrorResponse<ErrorResponse>();
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
        }

        [Fact]
        public async Task TestErrorAsync()
        {
            // arrange
            ExpectGetAllReviewsAndReturnError();

            var reviewSvc = CreateMovieReviewService();

            // act
            var exception =
                await Assert.ThrowsAsync<RestClientException>(() => reviewSvc.GetAllReviews(MovieId));

            // assert
            _mockHttp.VerifyNoOutstandingExpectation();

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            var error = exception.GetErrorResponse<ErrorResponse>();
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
        }

        private void ExpectGetAllReviewsAndReturnError(string movieId = MovieId)
        {
            _mockHttp
                .Expect(HttpMethod.Get, $"{BaseUri}/movies/{movieId}/reviews")
                .Respond(HttpStatusCode.NotFound, request => new StringContent(JsonConvert.SerializeObject(new
                {
                    Errors = new object[]
                        {
                            new {Message = "Sorry, that page does not exist", Code = 34}
                        }
                }),
                    Encoding.UTF8,
                    "application/json"));
        }

        [Fact]
        public void TestErrorSync()
        {
            // arrange
            _mockHttp
                .When(HttpMethod.Get, $"{BaseUri}/movies/{MovieId}/reviews/{ReviewId}")
                .Respond(HttpStatusCode.NotFound, request => new StringContent(JsonConvert.SerializeObject(new
                {
                    Errors = new object[]
                        {
                            new {Message = "Sorry, that page does not exist", Code = 34}
                        }
                }),
                    Encoding.UTF8,
                    "application/json"));

            var reviewSvc = CreateMovieReviewService();

            // act
            var aggregateException = Assert.Throws<AggregateException>(() => reviewSvc.GetReview(MovieId, ReviewId));

            // assert
            var exception = (RestClientException)aggregateException.GetBaseException();
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);

            dynamic dynamicError = exception.ErrorResponse;
            string message = dynamicError.Errors[0].Message;
            int code = dynamicError.Errors[0].Code;
            Assert.Equal(34, code);
            Assert.Equal("Sorry, that page does not exist", message);

            var error = exception.GetErrorResponse<ErrorResponse>();
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
        }


        [Fact]
        public async Task TestTimeoutAsync()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond(async () =>
                {
                    await Task.Delay(1000);
                    return null;
                });

            var httpClient = _mockHttp.ToHttpClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(1);
            var reviewSvc = _restClientFactory.CreateBuilder()
                .With(httpClient)
                .BaseUri(new Uri(BaseUri))
                .Build<IMovieReviewService>();

            // act
            await Assert.ThrowsAsync<TaskCanceledException>(() => reviewSvc.GetAllMovies());

            // assert
        }

        [Fact]
        public async Task TestCancellationAsync()
        {
            // arrange
            _mockHttp.When($"{BaseUri}/movies")
                .Respond(_ => null);

            var reviewSvc = CreateMovieReviewService();
            var cancellationTokenSource = new CancellationTokenSource();

            // act
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                reviewSvc.GetAllMoviesCancellable(cancellationTokenSource.Token));

            // assert
        }

        [Fact]
        public async Task TestNoCancellationAsync()
        {
            // arrange
            _mockHttp.When($"{BaseUri}/movies")
                .Respond("application/json", "[]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var movies = await reviewSvc.GetAllMoviesCancellable(default);

            // assert
            Assert.Empty(movies);
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
        public async Task TestGetEmptyIEnumerableAsync()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .WithHeaders("Accept", "application/json")
                .Respond("application/json", "[]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var movies = await reviewSvc.GetAllMovies();

            // assert
            Assert.Empty(movies);
        }

        [Fact]
        public async Task TestQueryParamAsync()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies?begin=2017-01-01T00%3A00%3A00.0000000Z&end=2018-01-01T00%3A00%3A00.0000000Z")
                .Respond("application/json", "[{\"Title\":\"Blade Runner 2049\"}]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var movies = await reviewSvc.QueryMoviesByDate(
                new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            // assert
            var list = movies.ToList();
            Assert.Single(list);
            var movie = list.First();
            Assert.Equal("Blade Runner 2049", movie.Title);
        }

        [Fact]
        public async Task TestPostJsonAsync()
        {
            // arrange
            var movieId = "FOOBAR";
            _mockHttp
                .When(HttpMethod.Post, $"{BaseUri}/movies/{movieId}/reviews")
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .Respond(request =>
                {
                    var content = request.Content.ReadAsStringAsync().Result;
                    content = content.Replace("\"ReviewId\":null", "\"ReviewId\":\"*REVIEW_ID*\"");
                    return new StringContent(content, Encoding.UTF8, "application/json");
                });

            var reviewSvc = CreateMovieReviewService();

            // act
            var text = "This was a delightful comedy, but not terribly realistic.";
            var stars = 3;
            var review = new Review(stars, text);
            var result = await reviewSvc.SubmitReview(movieId, review);

            // assert
            Assert.Equal("*REVIEW_ID*", result.ReviewId);
            Assert.Equal(stars, result.Stars);
            Assert.Equal(text, result.Text);
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
        public void TestPutSync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            var reviewId = "*REVIEW_ID*";
            _mockHttp
                .When(HttpMethod.Put, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
                .Respond(request => request.Content);

            var reviewSvc = CreateMovieReviewService();

            // act
            var text = "This was actally really good!";
            var stars = 5;
            var review = new Review(stars, text)
            {
                MovieId = movieId,
                ReviewId = reviewId
            };
            var result = reviewSvc.UpdateReview(movieId, reviewId, review);

            // assert
            Assert.Equal(movieId, result.MovieId);
            Assert.Equal(reviewId, result.ReviewId);
            Assert.Equal(stars, result.Stars);
            Assert.Equal(text, result.Text);
        }

        [Fact]
        public void TestPatchSync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            var reviewId = "*REVIEW_ID*";
            _mockHttp
                .When(HttpMethod.Patch, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
                .Respond(request => request.Content);

            var reviewSvc = CreateMovieReviewService();

            // act
            var text = "This was actally really good!";
            var stars = 5;
            var review = new Review(stars, text)
            {
                MovieId = movieId,
                ReviewId = reviewId
            };
            var result = reviewSvc.PartialUpdateReview(movieId, reviewId, review);

            // assert
            Assert.Equal(movieId, result.MovieId);
            Assert.Equal(reviewId, result.ReviewId);
            Assert.Equal(stars, result.Stars);
            Assert.Equal(text, result.Text);
        }

        [Fact]
        public async Task TestGetHttpContent()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond("application/json", "[]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var httpContent = reviewSvc.GetHttpContent();

            // assert
            Assert.Equal("[]", await httpContent.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestGetHttpResponseMessage()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond("application/json", "[]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var httpResponseMessage = reviewSvc.GetHttpResponseMessage();

            // assert
            Assert.Equal("[]", await httpResponseMessage.Content.ReadAsStringAsync());
        }

        [Fact]
        public void TestGetJObject()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/object")
                .Respond("application/json", "{\"foo\":\"bar\"}");

            var reviewSvc = CreateMovieReviewService();

            // act
            dynamic jObject = reviewSvc.GetJObject();

            // assert
            string foo = jObject.foo;
            Assert.Equal("bar", foo);
        }

        [Fact]
        public async Task TestGetJArray()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies/array")
                .Respond("application/json", "[{\"foo\":\"bar\"}]");

            var reviewSvc = CreateMovieReviewService();

            // act
            dynamic jArray = await reviewSvc.GetJArray();

            // assert
            string foo = jArray[0].foo;
            Assert.Equal("bar", foo);
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
        public async Task TestRequestLogger()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
                .Respond("application/json", "[]");

            var requestLoggerMock = new Mock<IRequestLogger>();
            requestLoggerMock.Setup(x => x.TimeOperation(It.IsAny<HttpRequestMessage>()))
                .Returns(() => new Mock<IDisposable>().Object);

            var reviewSvc = CreateRestClientBuilder()
                .With(requestLoggerMock.Object)
                .Build<IMovieReviewService>();

            // act
            await reviewSvc.GetAllMovies();
            await reviewSvc.GetAllMovies();

            // assert
            requestLoggerMock.Verify(x => x.TimeOperation(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
            requestLoggerMock.VerifyNoOtherCalls();
            _mockHttp.VerifyNoOutstandingExpectation();
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