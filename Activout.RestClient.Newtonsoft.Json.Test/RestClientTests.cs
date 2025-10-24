#nullable disable
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Newtonsoft.Json.Test.MovieReviews;
using Microsoft.Extensions.Logging;
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
            _restClientFactory = new RestClientFactory();
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
            Assert.NotNull(error);
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
        }

        private void ExpectGetAllReviewsAndReturnError(string movieId = MovieId)
        {
            _mockHttp
                .Expect(HttpMethod.Get, $"{BaseUri}/movies/{movieId}/reviews")
                .Respond(HttpStatusCode.NotFound, _ => new StringContent(JsonConvert.SerializeObject(new
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

            Assert.NotNull(exception.ErrorResponse);
            dynamic dynamicError = exception.ErrorResponse;
            string message = dynamicError.Errors[0].Message;
            int code = dynamicError.Errors[0].Code;
            Assert.Equal(34, code);
            Assert.Equal("Sorry, that page does not exist", message);

            var error = exception.GetErrorResponse<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
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
            _mockHttp
                .When(HttpMethod.Post, $"{BaseUri}/movies/FOOBAR/reviews")
                .WithHeaders("Content-Type", "application/json; charset=utf-8")
                .Respond(request =>
                {
                    var content = request.Content!.ReadAsStringAsync().Result;
                    content = content.Replace("{", "{\"ReviewId\":\"*REVIEW_ID*\", ");
                    return new StringContent(content, Encoding.UTF8, "application/json");
                });

            var reviewSvc = CreateMovieReviewService();

            // act
            var review = new Review(3, "This was a delightful comedy, but not terribly realistic.");
            var result = await reviewSvc.SubmitReview("FOOBAR", review);

            // assert
            Assert.Equal("*REVIEW_ID*", result.ReviewId);
            Assert.Equal(3, result.Stars);
            Assert.Equal("This was a delightful comedy, but not terribly realistic.", result.Text);
        }

        [Fact]
        public void TestPutSync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            var reviewId = "*REVIEW_ID*";
            _mockHttp
                .When(HttpMethod.Put, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
                .Respond(request => request.Content!);

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
                .Respond(request => request.Content!);

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
    }
}