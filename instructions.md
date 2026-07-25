# Using Activout.RestClient in Your Project

This guide shows you how to use Activout.RestClient to create type-safe REST API clients by defining C# interfaces, and how to unit test them using MockHttp.

## Installation

Install the required packages from NuGet:

```bash
# Core package
dotnet add package Activout.RestClient

# Choose one JSON serialization package:
dotnet add package Activout.RestClient.Json              # For System.Text.Json
dotnet add package Activout.RestClient.Newtonsoft.Json   # For Newtonsoft.Json

# For unit testing with MockHttp
dotnet add package RichardSzalay.MockHttp --version 7.0.0
dotnet add package xunit
```

## Step 1: Define Your API Interface

Create an interface that describes your REST API endpoints. Use attributes to specify HTTP methods, paths, and parameters.

```csharp
using Activout.RestClient;

[Path("movies")]
[ErrorResponse(typeof(ErrorResponse))]
[Accept("application/json")]
[ContentType("application/json")]
public interface IMovieReviewService
{
    // GET /movies
    [Get]
    Task<IEnumerable<Movie>> GetAllMovies();

    // GET /movies/{movieId}/reviews
    [Get("/{movieId}/reviews")]
    Task<IEnumerable<Review>> GetAllReviews(string movieId);

    // GET /movies/{movieId}/reviews/{reviewId}
    [Get("/{movieId}/reviews/{reviewId}")]
    Task<Review> GetReview(string movieId, string reviewId);

    // POST /movies/{movieId}/reviews
    [Post("/{movieId}/reviews")]
    Task<Review> SubmitReview(string movieId, Review review);

    // PUT /movies/{movieId}/reviews/{reviewId}
    [Put("/{movieId}/reviews/{reviewId}")]
    Task<Review> UpdateReview(string movieId, string reviewId, Review review);

    // DELETE /movies/{movieId}/reviews/{reviewId}
    [Delete("/{movieId}/reviews/{reviewId}")]
    Task DeleteReview(string movieId, string reviewId);

    // GET /movies?begin=...&end=...
    [Get]
    Task<IEnumerable<Movie>> QueryMoviesByDate(
        [QueryParam] DateTime begin,
        [QueryParam] DateTime end);
}
```

### Available Attributes

**Interface-level attributes:**
- `[Path("base/path")]` - Base path for all methods in the interface
- `[Accept("application/json")]` - Default Accept header
- `[ContentType("application/json")]` - Default Content-Type for POST/PUT requests
- `[ErrorResponse(typeof(ErrorResponse))]` - Type used to deserialize error responses

**Method-level attributes:**
- `[Get]`, `[Post]`, `[Put]`, `[Delete]`, `[Patch]` - HTTP method
- `[Path("relative/path")]` - Relative path for the method

**Parameter attributes:**
- `[PathParam]` - Path parameter (in URL path with `{paramName}`)
- `[QueryParam]` - Query string parameter (`?key=value`)
- `[HeaderParam]` - HTTP header parameter
- `[FormParam]` - Form data parameter (for `application/x-www-form-urlencoded`)
- `[PartParam]` - Multipart form data parameter

## Step 2: Define Data Models

Define your request and response models as regular C# classes or records:

```csharp
public class Movie
{
    public string? Title { get; init; }
}

public class Review
{
    public Review(int stars, string text)
    {
        Stars = stars;
        Text = text;
    }

    public string? MovieId { get; init; }
    public string? ReviewId { get; init; }
    public int Stars { get; init; }
    public string Text { get; init; }
}

public class ErrorResponse
{
    public List<Error> Errors { get; init; } = [];

    public class Error
    {
        public int Code { get; init; }
        public string? Message { get; init; }
    }
}
```

## Step 3: Create and Use the REST Client

### Using System.Text.Json

```csharp
using Activout.RestClient;
using Activout.RestClient.Json;

var restClientFactory = new RestClientFactory();
var movieService = restClientFactory
    .CreateBuilder()
    .With(httpClient)                    // Use your HttpClient
    .WithSystemTextJson()                // Enable System.Text.Json serialization
    .BaseUri(new Uri("https://api.example.com"))
    .Build<IMovieReviewService>();

// Use the client
var movies = await movieService.GetAllMovies();
var review = new Review(5, "Amazing movie!");
await movieService.SubmitReview("movie-123", review);
```

### Using Newtonsoft.Json

```csharp
using Activout.RestClient;
using Activout.RestClient.Newtonsoft.Json;

var restClientFactory = new RestClientFactory();
var movieService = restClientFactory
    .CreateBuilder()
    .With(httpClient)
    .WithNewtonsoftJson()                // Enable Newtonsoft.Json serialization
    .BaseUri(new Uri("https://api.example.com"))
    .Build<IMovieReviewService>();

var movies = await movieService.GetAllMovies();
```

### Using with Dependency Injection

You can register the required RestClient services and your API clients in your DI container. Here's an example using ASP.NET Core's `IServiceCollection`:

```csharp
using Activout.RestClient;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.ParamConverter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRestClient(this IServiceCollection services)
    {
        services.TryAddTransient<IDuckTyping, DuckTyping>();
        services.TryAddTransient<IParamConverterManager, ParamConverterManager>();
        services.TryAddTransient<IRestClientFactory, RestClientFactory>();
        services.TryAddTransient<ITaskConverterFactory, TaskConverter3Factory>();
        return services;
    }
}

// In your Program.cs or Startup.cs
services.AddRestClient();
services.AddHttpClient();

// Register your specific API client
services.AddSingleton<IMovieReviewService>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var restClientFactory = provider.GetRequiredService<IRestClientFactory>();
    
    return restClientFactory.CreateBuilder()
        .With(httpClient)
        .WithSystemTextJson()
        .BaseUri(new Uri("https://api.example.com"))
        .Build<IMovieReviewService>();
});
```

**Note:** The core Activout.RestClient library doesn't include built-in DI extensions. The above code demonstrates how to create your own extension methods to register the necessary services.

## Step 4: Unit Testing with MockHttp

Use `RichardSzalay.MockHttp` to mock HTTP responses in your unit tests without making real HTTP calls.

### Basic Test Setup

```csharp
using Activout.RestClient;
using Activout.RestClient.Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

public class MovieReviewServiceTests
{
    private const string BaseUri = "https://api.example.com";
    private readonly IRestClientFactory _restClientFactory;
    private readonly MockHttpMessageHandler _mockHttp;

    public MovieReviewServiceTests()
    {
        _restClientFactory = new RestClientFactory();
        _mockHttp = new MockHttpMessageHandler();
    }

    private IMovieReviewService CreateMovieReviewService()
    {
        return _restClientFactory.CreateBuilder()
            .WithNewtonsoftJson()
            .With(_mockHttp.ToHttpClient())           // Use mock HTTP client
            .BaseUri(BaseUri)
            .Build<IMovieReviewService>();
    }
}
```

### Testing GET Requests

```csharp
[Fact]
public async Task GetAllMovies_ReturnsMovieList()
{
    // Arrange
    _mockHttp
        .When($"{BaseUri}/movies")
        .WithHeaders("Accept", "application/json")
        .Respond("application/json", "[{\"Title\":\"Inception\"}]");

    var service = CreateMovieReviewService();

    // Act
    var movies = await service.GetAllMovies();

    // Assert
    var movieList = movies.ToList();
    Assert.Single(movieList);
    Assert.Equal("Inception", movieList[0].Title);
}

[Fact]
public async Task GetAllMovies_ReturnsEmptyList_WhenNoMovies()
{
    // Arrange
    _mockHttp
        .When($"{BaseUri}/movies")
        .Respond("application/json", "[]");

    var service = CreateMovieReviewService();

    // Act
    var movies = await service.GetAllMovies();

    // Assert
    Assert.Empty(movies);
}
```

### Testing POST Requests

```csharp
[Fact]
public async Task SubmitReview_CreatesNewReview()
{
    // Arrange
    var movieId = "movie-123";
    _mockHttp
        .When(HttpMethod.Post, $"{BaseUri}/movies/{movieId}/reviews")
        .WithHeaders("Content-Type", "application/json; charset=utf-8")
        .Respond(request =>
        {
            // Echo back the request with an added ReviewId
            var content = request.Content!.ReadAsStringAsync().Result;
            content = content.Replace("{", "{\"ReviewId\":\"review-456\", ");
            return new StringContent(content, Encoding.UTF8, "application/json");
        });

    var service = CreateMovieReviewService();
    var review = new Review(5, "Excellent movie!");

    // Act
    var result = await service.SubmitReview(movieId, review);

    // Assert
    Assert.Equal("review-456", result.ReviewId);
    Assert.Equal(5, result.Stars);
    Assert.Equal("Excellent movie!", result.Text);
}
```

### Testing PUT Requests

```csharp
[Fact]
public async Task UpdateReview_ModifiesExistingReview()
{
    // Arrange
    var movieId = "movie-123";
    var reviewId = "review-456";
    _mockHttp
        .When(HttpMethod.Put, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
        .Respond(request => request.Content!);  // Echo back the request

    var service = CreateMovieReviewService();
    var updatedReview = new Review(4, "Good, but not great");

    // Act
    var result = await service.UpdateReview(movieId, reviewId, updatedReview);

    // Assert
    Assert.Equal(4, result.Stars);
    Assert.Equal("Good, but not great", result.Text);
}
```

### Testing Query Parameters

```csharp
[Fact]
public async Task QueryMoviesByDate_UsesQueryParameters()
{
    // Arrange
    var begin = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    var end = new DateTime(2020, 12, 31, 0, 0, 0, DateTimeKind.Utc);
    
    _mockHttp
        .When($"{BaseUri}/movies?begin=2020-01-01T00%3A00%3A00.0000000Z&end=2020-12-31T00%3A00%3A00.0000000Z")
        .Respond("application/json", "[{\"Title\":\"Tenet\"}]");

    var service = CreateMovieReviewService();

    // Act
    var movies = await service.QueryMoviesByDate(begin, end);

    // Assert
    var movieList = movies.ToList();
    Assert.Single(movieList);
    Assert.Equal("Tenet", movieList[0].Title);
}
```

### Testing Error Responses

```csharp
[Fact]
public async Task GetReview_ThrowsRestClientException_WhenNotFound()
{
    // Arrange
    var movieId = "movie-123";
    var reviewId = "invalid-review";
    
    _mockHttp
        .Expect(HttpMethod.Get, $"{BaseUri}/movies/{movieId}/reviews/{reviewId}")
        .Respond(HttpStatusCode.NotFound, _ => new StringContent(
            JsonConvert.SerializeObject(new
            {
                Errors = new[]
                {
                    new { Code = 404, Message = "Review not found" }
                }
            }),
            Encoding.UTF8,
            "application/json"));

    var service = CreateMovieReviewService();

    // Act & Assert
    var exception = await Assert.ThrowsAsync<RestClientException>(
        () => service.GetReview(movieId, reviewId));
    
    Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    
    var error = exception.GetErrorResponse<ErrorResponse>();
    Assert.NotNull(error);
    Assert.Equal(404, error.Errors[0].Code);
    Assert.Equal("Review not found", error.Errors[0].Message);
    
    // Verify the expected call was made
    _mockHttp.VerifyNoOutstandingExpectation();
}
```

### Using Expect vs When

MockHttp provides two ways to set up expectations:

**`When()`** - Matches any number of requests:
```csharp
_mockHttp.When($"{BaseUri}/movies")
    .Respond("application/json", "[]");
```

**`Expect()`** - Expects exactly one request:
```csharp
_mockHttp.Expect($"{BaseUri}/movies")
    .Respond("application/json", "[]");

// Verify all expectations were met
_mockHttp.VerifyNoOutstandingExpectation();
```

### Testing with Custom JSON Settings

For System.Text.Json:
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using Activout.RestClient.Json;

var jsonOptions = new JsonSerializerOptions(SystemTextJsonDefaults.SerializerOptions)
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var service = _restClientFactory.CreateBuilder()
    .WithSystemTextJson(jsonOptions)
    .With(_mockHttp.ToHttpClient())
    .BaseUri(BaseUri)
    .Build<IMovieReviewService>();
```

For Newtonsoft.Json:
```csharp
using Newtonsoft.Json;
using Activout.RestClient.Newtonsoft.Json;

var jsonSettings = new JsonSerializerSettings(NewtonsoftJsonDefaults.DefaultJsonSerializerSettings)
{
    NullValueHandling = NullValueHandling.Ignore
};

var service = _restClientFactory.CreateBuilder()
    .WithNewtonsoftJson(jsonSettings)
    .With(_mockHttp.ToHttpClient())
    .BaseUri(BaseUri)
    .Build<IMovieReviewService>();
```

## Complete Test Example

Here's a complete test class demonstrating various scenarios:

```csharp
using System.Net;
using System.Text;
using Activout.RestClient;
using Activout.RestClient.Newtonsoft.Json;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace MyProject.Tests;

public class MovieReviewServiceTests
{
    private const string BaseUri = "https://api.example.com";
    private readonly IRestClientFactory _restClientFactory;
    private readonly MockHttpMessageHandler _mockHttp;

    public MovieReviewServiceTests()
    {
        _restClientFactory = new RestClientFactory();
        _mockHttp = new MockHttpMessageHandler();
    }

    private IMovieReviewService CreateService()
    {
        return _restClientFactory.CreateBuilder()
            .WithNewtonsoftJson()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri)
            .Build<IMovieReviewService>();
    }

    [Fact]
    public async Task GetAllMovies_Success()
    {
        _mockHttp
            .When($"{BaseUri}/movies")
            .Respond("application/json", "[{\"Title\":\"Inception\"}]");

        var service = CreateService();
        var movies = await service.GetAllMovies();

        Assert.Single(movies);
    }

    [Fact]
    public async Task SubmitReview_Success()
    {
        _mockHttp
            .When(HttpMethod.Post, $"{BaseUri}/movies/123/reviews")
            .Respond(request =>
            {
                var content = request.Content!.ReadAsStringAsync().Result;
                content = content.Replace("{", "{\"ReviewId\":\"456\", ");
                return new StringContent(content, Encoding.UTF8, "application/json");
            });

        var service = CreateService();
        var review = new Review(5, "Great!");
        var result = await service.SubmitReview("123", review);

        Assert.Equal("456", result.ReviewId);
        Assert.Equal(5, result.Stars);
    }

    [Fact]
    public async Task GetReview_NotFound_ThrowsException()
    {
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/movies/123/reviews/999")
            .Respond(HttpStatusCode.NotFound, _ => new StringContent(
                JsonConvert.SerializeObject(new
                {
                    Errors = new[] { new { Code = 404, Message = "Not found" } }
                }),
                Encoding.UTF8,
                "application/json"));

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RestClientException>(
            () => service.GetReview("123", "999"));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var error = exception.GetErrorResponse<ErrorResponse>();
        Assert.Equal(404, error!.Errors[0].Code);
    }
}
```

## Advanced Features

### Custom Headers

```csharp
[Get("/secure-endpoint")]
[Header("Authorization", "Bearer {token}")]
Task<Data> GetSecureData(string token);

// Or use HeaderParam for dynamic headers
[Get("/data")]
Task<Data> GetData([HeaderParam("X-Custom-Header")] string customValue);
```

### Form Data

```csharp
[Post("/login")]
[ContentType("application/x-www-form-urlencoded")]
Task<LoginResponse> Login([FormParam] string username, [FormParam] string password);
```

### Multipart Form Data

```csharp
[Post("/upload")]
[ContentType("multipart/form-data")]
Task<UploadResponse> UploadFile([PartParam] string description, [PartParam] Stream file);
```

### Synchronous Methods

Both synchronous and asynchronous methods are supported:

```csharp
// Async (recommended)
Task<Movie> GetMovieAsync(string id);

// Sync
Movie GetMovie(string id);
```

## Troubleshooting

### Common Issues

1. **Null values in parameters**: Null values mean no parameter is sent. Use empty string to send a parameter without a value.

2. **JSON serialization errors**: Ensure you've installed and configured either `Activout.RestClient.Json` or `Activout.RestClient.Newtonsoft.Json`.

3. **Path parameter not replaced**: Ensure the parameter name in `{paramName}` matches the method parameter name or use `[PathParam("paramName")]`.

4. **MockHttp not matching requests**: Check the exact URL, headers, and HTTP method. Use `_mockHttp.GetMatchCount()` to debug.

## Additional Resources

- [Activout.RestClient GitHub Repository](https://github.com/twogood/Activout.RestClient)
- [RichardSzalay.MockHttp Documentation](https://github.com/richardszalay/mockhttp)
- [System.Text.Json Documentation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [Newtonsoft.Json Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)

## Support

Need help implementing Activout.RestClient? Contact [david@activout.se](mailto:david@activout.se) to order a support package.

## License

Activout.RestClient is licensed under the [MIT License](https://github.com/twogood/Activout.RestClient/blob/main/LICENSE).
