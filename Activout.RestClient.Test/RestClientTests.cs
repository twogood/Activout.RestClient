using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.MovieReviews;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    public class RestClientTests
    {
        public RestClientTests()
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        private const string BaseUri = "http://localhost:9080/movieReviewService";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        private IMovieReviewService CreateMovieReviewService()
        {
            return _restClientFactory.CreateBuilder()
                .HttpClient(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri))
                .Build<IMovieReviewService>();
        }

        [Fact]
        public async Task TestErrorAsync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            _mockHttp
                .When(HttpMethod.Get, $"{BaseUri}/movies/{movieId}/reviews")
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
            var aggregateException =
                await Assert.ThrowsAsync<AggregateException>(() => reviewSvc.GetAllReviews(movieId));

            // assert
            Assert.IsType<RestClientException>(aggregateException.InnerException);
            var exception = (RestClientException) aggregateException.InnerException;

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            var error = exception.GetErrorResponse<ErrorResponse>();
            Assert.Equal(34, error.Errors[0].Code);
            Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
        }

        [Fact]
        public void TestErrorSync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            var reviewId = "*REVIEW_ID*";
            _mockHttp
                .When(HttpMethod.Get, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
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
            var aggregateException = Assert.Throws<AggregateException>(() => reviewSvc.GetReview(movieId, reviewId));

            // assert
            var exception = (RestClientException) aggregateException.GetBaseException();
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
        public async Task TestGetEmptyIEnumerableAsync()
        {
            // arrange
            _mockHttp
                .When($"{BaseUri}/movies")
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

    }
}