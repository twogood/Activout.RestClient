using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.Helpers;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Newtonsoft.Json.Test.MovieReviews;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.ParamConverter.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Newtonsoft.Json.Test;

public class HttpClientFactoryWithDelegatingHandlerTest
{
    private const string BaseUri = "https://example.com/movieReviewService";
    private const string MovieId = "test-movie-123";

    private readonly ITestOutputHelper _outputHelper;
    private readonly MockHttpMessageHandler _mockHttp = new();

    public HttpClientFactoryWithDelegatingHandlerTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    private class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ITestOutputHelper _outputHelper;

        public LoggingDelegatingHandler(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _outputHelper.WriteLine($"[Request] {request.Method} {request.RequestUri}");

            if (request.Content != null)
            {
                // Buffer the content so we can read it multiple times
                await request.Content.LoadIntoBufferAsync();
                var requestContent = await request.Content.ReadAsStringAsync(cancellationToken);
                _outputHelper.WriteLine($"[Request Content] {requestContent}");
            }

            var response = await base.SendAsync(request, cancellationToken);

            _outputHelper.WriteLine($"[Response] {response.StatusCode}");

            await response.Content.LoadIntoBufferAsync();
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _outputHelper.WriteLine($"[Response Content] {responseContent}");

            return response;
        }
    }

    private class MovieReviewServiceFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IRestClientFactory _restClientFactory;
        private readonly ILogger<MovieReviewServiceFactory> _logger;

        public MovieReviewServiceFactory(
            HttpClient httpClient,
            IRestClientFactory restClientFactory,
            ILogger<MovieReviewServiceFactory> logger)
        {
            _httpClient = httpClient;
            _restClientFactory = restClientFactory;
            _logger = logger;
        }

        public IMovieReviewService CreateMovieReviewService()
        {
            return _restClientFactory.CreateBuilder()
                .WithNewtonsoftJson()
                .With(_logger)
                .With(_httpClient)
                .BaseUri(BaseUri)
                .Build<IMovieReviewService>();
        }
    }

    private static IServiceCollection AddRestClient(IServiceCollection services)
    {
        services.TryAddTransient<IDuckTyping, DuckTyping>();
        services.TryAddTransient<IParamConverterManager, ParamConverterManager>();
        services.TryAddTransient<IRestClientFactory, RestClientFactory>();
        services.TryAddTransient<ITaskConverterFactory, TaskConverter3Factory>();
        return services;
    }

    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<HttpMessageHandlerBuilder>(_ => new MockHttpMessageHandlerBuilder(_mockHttp));

        AddRestClient(services);

        services.AddLogging(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Activout.RestClient", LogLevel.Debug)
                .AddXUnit(_outputHelper);
        });

        services.AddHttpClient<MovieReviewServiceFactory>()
            .AddHttpMessageHandler(() => new LoggingDelegatingHandler(_outputHelper));

        return services.BuildServiceProvider();
    }

    private IMovieReviewService CreateMovieReviewService()
    {
        var serviceProvider = CreateServiceProvider();
        var factory = serviceProvider.GetRequiredService<MovieReviewServiceFactory>();
        return factory.CreateMovieReviewService();
    }

    private IMovieReviewService CreateMovieReviewServiceWithConfiguredPrimaryHandler()
    {
        var services = new ServiceCollection();

        AddRestClient(services);

        services.AddLogging(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Activout.RestClient", LogLevel.Debug)
                .AddXUnit(_outputHelper);
        });

        services.AddHttpClient<MovieReviewServiceFactory>()
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttp)
            .AddHttpMessageHandler(() => new LoggingDelegatingHandler(_outputHelper));

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<MovieReviewServiceFactory>();
        return factory.CreateMovieReviewService();
    }

    [Fact]
    public async Task TestGetWithDelegatingHandler_ShouldDeserializeSuccessfully()
    {
        // Arrange
        var movies = new[]
        {
            new Movie { Title = "Test Movie 1" },
            new Movie { Title = "Test Movie 2" }
        };

        _mockHttp
            .Expect(HttpMethod.Get, $"{BaseUri}/movies")
            .Respond("application/json", JsonConvert.SerializeObject(movies));

        var reviewSvc = CreateMovieReviewServiceWithConfiguredPrimaryHandler();

        // Act
        var result = await reviewSvc.GetAllMovies();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task TestGetReviewsWithDelegatingHandler_ShouldDeserializeSuccessfully()
    {
        // Arrange
        var reviews = new[]
        {
            new Review(5, "Great movie!") { MovieId = MovieId, ReviewId = "rev1" },
            new Review(4, "Good movie") { MovieId = MovieId, ReviewId = "rev2" }
        };

        _mockHttp
            .Expect(HttpMethod.Get, $"{BaseUri}/movies/{MovieId}/reviews")
            .Respond("application/json", JsonConvert.SerializeObject(reviews));

        var reviewSvc = CreateMovieReviewService();

        var result = await reviewSvc.GetAllReviews(MovieId);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task TestUnsafeWhenWithReusedHttpContent_SecondCallThrowsObjectDisposedException()
    {
        // Arrange
        var movies = new[]
        {
            new Movie { Title = "Test Movie 1" },
            new Movie { Title = "Test Movie 2" }
        };
        var sharedContent = new StringContent(
            JsonConvert.SerializeObject(movies),
            Encoding.UTF8,
            "application/json");

        // Intentionally unsafe: same HttpContent instance is returned for every request.
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/movies")
            .Respond(_ => sharedContent);

        var reviewSvc = CreateMovieReviewService();

        // Act
        var firstCallResult = await reviewSvc.GetAllMovies();

        // Assert first call succeeds, second call reuses disposed content and fails.
        Assert.NotNull(firstCallResult);
        Assert.Equal(2, firstCallResult.Count());

        await Assert.ThrowsAsync<ObjectDisposedException>(() => reviewSvc.GetAllMovies());
    }

    [Fact]
    public async Task TestPostWithDelegatingHandler_ShouldSubmitReview()
    {
        // Arrange
        var review = new Review(5, "Amazing!") { MovieId = MovieId, ReviewId = "new-review" };

        _mockHttp
            .Expect(HttpMethod.Post, $"{BaseUri}/movies/{MovieId}/reviews")
            .Respond("application/json", JsonConvert.SerializeObject(review));

        var reviewSvc = CreateMovieReviewService();

        // Act
        var result = await reviewSvc.SubmitReview(MovieId, review);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.NotNull(result);
        Assert.Equal(review.ReviewId, result.ReviewId);
    }

    [Fact]
    public async Task TestErrorResponseWithDelegatingHandler_ShouldDeserializeErrorResponse()
    {
        // Arrange
        _mockHttp
            .Expect(HttpMethod.Get, $"{BaseUri}/movies/{MovieId}/reviews")
            .Respond(HttpStatusCode.NotFound, _ => new StringContent(
                JsonConvert.SerializeObject(new
                {
                    Errors = new object[]
                    {
                        new { Message = "Movie not found", Code = 404 }
                    }
                }),
                Encoding.UTF8,
                "application/json"));

        var reviewSvc = CreateMovieReviewService();

        // Act & Assert
        // The error response also goes through the delegating handler
        // and needs to be deserialized, so this is another path where
        // ObjectDisposedException could occur
        var exception = await Assert.ThrowsAsync<RestClientException>(() => reviewSvc.GetAllReviews(MovieId));

        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);

        var error = exception.GetErrorResponse<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Single(error.Errors);
        Assert.Equal(404, error.Errors[0].Code);
    }
}