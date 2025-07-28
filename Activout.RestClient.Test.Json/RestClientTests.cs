using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Json;
using Activout.RestClient.Newtonsoft.Json;
using Activout.RestClient.Test.Json.MovieReviews;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test.Json;

public enum JsonImplementation
{
    SystemTextJson,
    NewtonsoftJson
}

public class RestClientTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/movieReviewService";
    private const string MovieId = "*MOVIE_ID*";
    private const string ReviewId = "*REVIEW_ID*";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private IRestClientBuilder CreateRestClientBuilder(JsonImplementation jsonImplementation)
    {
        var builder = _restClientFactory.CreateBuilder();
        
        return jsonImplementation switch
        {
            JsonImplementation.SystemTextJson => builder.WithSystemTextJson(),
            JsonImplementation.NewtonsoftJson => builder.WithNewtonsoftJson(),
            _ => throw new ArgumentOutOfRangeException(nameof(jsonImplementation))
        };
    }

    private IMovieReviewService CreateMovieReviewService(JsonImplementation jsonImplementation)
    {
        return CreateRestClientBuilder(jsonImplementation)
            .Accept("application/json")
            .ContentType("application/json")
            .With(_loggerFactory.CreateLogger<RestClientTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<IMovieReviewService>();
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestErrorAsyncWithOldTaskConverter(JsonImplementation jsonImplementation)
    {
        // arrange
        ExpectGetAllReviewsAndReturnError();

        var reviewSvc = CreateRestClientBuilder(jsonImplementation)
            .Accept("application/json")
            .ContentType("application/json")
            .With(new TaskConverterFactory())
            .With(_loggerFactory.CreateLogger<RestClientTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<IMovieReviewService>();

        // act
        var aggregateException =
            await Assert.ThrowsAsync<AggregateException>(() => reviewSvc.GetAllReviews(MovieId));

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();

        Assert.IsType<RestClientException>(aggregateException.InnerException);
        var exception = (RestClientException)aggregateException.InnerException!;

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var error = exception.GetErrorResponse<ErrorResponse>();
        Assert.Equal(34, error.Errors[0].Code);
        Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestErrorAsync(JsonImplementation jsonImplementation)
    {
        // arrange
        ExpectGetAllReviewsAndReturnError();

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

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

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public void TestErrorSync(JsonImplementation jsonImplementation)
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

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

        // act
        var aggregateException = Assert.Throws<AggregateException>(() => reviewSvc.GetReview(MovieId, ReviewId));

        // assert
        var exception = (RestClientException)aggregateException.GetBaseException();
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);

        dynamic dynamicError = exception.ErrorResponse!;
        string message = dynamicError.Errors[0].Message;
        int code = dynamicError.Errors[0].Code;
        Assert.Equal(34, code);
        Assert.Equal("Sorry, that page does not exist", message);

        var error = exception.GetErrorResponse<ErrorResponse>();
        Assert.Equal(34, error.Errors[0].Code);
        Assert.Equal("Sorry, that page does not exist", error.Errors[0].Message);
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestGetEmptyIEnumerableAsync(JsonImplementation jsonImplementation)
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies")
            .WithHeaders("Accept", "application/json")
            .Respond("application/json", "[]");

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

        // act
        var movies = await reviewSvc.GetAllMovies();

        // assert
        Assert.Empty(movies);
    }

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestQueryParamAsync(JsonImplementation jsonImplementation)
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies?begin=2017-01-01T00%3A00%3A00.0000000Z&end=2018-01-01T00%3A00%3A00.0000000Z")
            .Respond("application/json", "[{\"Title\":\"Blade Runner 2049\"}]");

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

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

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public async Task TestPostJsonAsync(JsonImplementation jsonImplementation)
    {
        // arrange
        var movieId = "FOOBAR";
        
        // The replacement logic needs to handle different JSON formats
        _mockHttp
            .When(HttpMethod.Post, $"{BaseUri}/movies/{movieId}/reviews")
            .WithHeaders("Content-Type", "application/json; charset=utf-8")
            .Respond(request =>
            {
                var content = request.Content!.ReadAsStringAsync().Result;
                
                // Handle different serialization formats
                content = jsonImplementation switch
                {
                    JsonImplementation.SystemTextJson => content.Replace("\"reviewId\":null", "\"reviewId\":\"*REVIEW_ID*\"")
                                                                .Replace("}", ",\"reviewId\":\"*REVIEW_ID*\"}"), // Add reviewId if not present
                    JsonImplementation.NewtonsoftJson => content.Replace("\"ReviewId\":null", "\"ReviewId\":\"*REVIEW_ID*\""),
                    _ => throw new ArgumentOutOfRangeException(nameof(jsonImplementation))
                };
                
                return new StringContent(content, Encoding.UTF8, "application/json");
            });

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

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

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public void TestPutSync(JsonImplementation jsonImplementation)
    {
        // arrange
        var movieId = "*MOVIE_ID*";
        var reviewId = "*REVIEW_ID*";
        _mockHttp
            .When(HttpMethod.Put, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
            .Respond(request => request.Content!);

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

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

    [Theory]
    [InlineData(JsonImplementation.SystemTextJson)]
    [InlineData(JsonImplementation.NewtonsoftJson)]
    public void TestPatchSync(JsonImplementation jsonImplementation)
    {
        // arrange
        var movieId = "*MOVIE_ID*";
        var reviewId = "*REVIEW_ID*";
        _mockHttp
            .When(HttpMethod.Patch, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
            .Respond(request => request.Content!);

        var reviewSvc = CreateMovieReviewService(jsonImplementation);

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
}