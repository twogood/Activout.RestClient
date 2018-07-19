using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Activout.MovieReviews;
using Activout.RestClient;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClientTest
{
    public class RestClientTests
    {
        public RestClientTests()
        {
            restClientFactory = Services.CreateRestClientFactory();
            mockHttp = new MockHttpMessageHandler();
        }

        private const string BASE_URI = "http://localhost:9080/movieReviewService";

        private readonly IRestClientFactory restClientFactory;
        private readonly MockHttpMessageHandler mockHttp;

        private IMovieReviewService CreateMovieReviewService()
        {
            return restClientFactory.CreateBuilder(mockHttp.ToHttpClient())
                .BaseUri(new Uri(BASE_URI))
                .Build<IMovieReviewService>();
        }

        [Fact]
        public async Task TestErrorAsync()
        {
            // arrange
            var movieId = "*MOVIE_ID*";
            mockHttp
                .When(HttpMethod.Get, $"{BASE_URI}/movies/{movieId}/reviews")
                .Respond(HttpStatusCode.NotFound, request =>
                {
                    return new StringContent(JsonConvert.SerializeObject(new
                        {
                            Errors = new object[]
                            {
                                new {Message = "Sorry, that page does not exist", Code = 34}
                            }
                        }),
                        Encoding.UTF8,
                        "application/json");
                    ;
                });

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
            mockHttp
                .When(HttpMethod.Get, $"{BASE_URI}/movies/{movieId}/reviews/{reviewId}")
                .Respond(HttpStatusCode.NotFound, request =>
                {
                    return new StringContent(JsonConvert.SerializeObject(new
                        {
                            Errors = new object[]
                            {
                                new {Message = "Sorry, that page does not exist", Code = 34}
                            }
                        }),
                        Encoding.UTF8,
                        "application/json");
                    ;
                });

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
            mockHttp
                .When($"{BASE_URI}/movies")
                .Respond("application/json", "[]");

            var reviewSvc = CreateMovieReviewService();

            // act
            var movies = await reviewSvc.GetAllMovies();

            // assert
            Assert.Empty(movies);
        }

        [Fact]
        public async Task TestPostJsonAsync()
        {
            // arrange
            var movieId = "FOOBAR";
            mockHttp
                .When(HttpMethod.Post, $"{BASE_URI}/movies/{movieId}/reviews")
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
            mockHttp
                .When(HttpMethod.Post, $"{BASE_URI}/movies/import.csv")
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
            mockHttp
                .When(HttpMethod.Put, $"{BASE_URI}/movies/{movieId}/reviews/{reviewId}")
                .Respond(request => { return request.Content; });

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
    }
}