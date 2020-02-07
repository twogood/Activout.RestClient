# Activout Rest Client
[![Build Status](https://travis-ci.org/twogood/Activout.RestClient.svg?branch=master)](https://travis-ci.org/twogood/Activout.RestClient)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=Activout.RestClient&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=Activout.RestClient)
[![NuGet Badge](https://buildstats.info/nuget/Activout.RestClient)](https://www.nuget.org/packages/Activout.RestClient/)

Create a REST(ish) API client only by defining the C# interface you want.

*Shamelessly inspired by [Rest Client for MicroProfile](https://github.com/eclipse/microprofile-rest-client).* 

## Rationale
The Activout Rest Client provides a type-safe approach to invoke RESTful services over HTTP. As much as possible the Rest Client attempts to use .NET Core MVC APIs for consistency and easier re-use.

## Example
Here is an example - let’s say that you want to use a movie review service. The remote service might provide APIs to view users' reviews and allow you to post and modify your own reviews. You might start with an interface to represent the remote service like this:

```C#
[Route("movies")]
[ErrorResponse(typeof(ErrorResponse))]
[Accept("application/json")]
[ContentType("application/json")]
public interface IMovieReviewService
{
    Task<List<Movie>> GetAllMovies();

    Task<List<Movie>> QueryMoviesByDate(
        [QueryParam] DateTime begin,
        [QueryParam] DateTime end);

    [Get("/{movieId}/reviews")]
    Task<IEnumerable<Review>> GetAllReviews(string movieId);

    [Get("/{movieId}/reviews/{reviewId}")]
    Task<Review> GetReview(string movieId, string reviewId);

    [Post("/{movieId}/reviews")]
    Task<Review> SubmitReview(string movieId, Review review);

    [Put("/{movieId}/reviews/{reviewId}")]
    Task<Review> UpdateReview(string movieId, string reviewId, Review review);

    [Post("/import.csv")]
    [ContentType("text/csv")]
    Task Import(string csv);
}
```

Now we can use this interface as a means to invoke the actual remote review service like this:

```C#
var restClientFactory = Services.CreateRestClientFactory();
var movieReviewService = restClientFactory
            .CreateBuilder()
            .HttpClient(_httpClient)
            .BaseUri(new Uri("http://localhost:9080/movieReviewService"))
            .Build<IMovieReviewService>();

Review review = new Review(stars: 3, "This was a delightful comedy, but not terribly realistic.");
await movieReviewService.SubmitReview(movieId, review);
```

This allows for a much more natural coding style, and the underlying implementation handles the communication between the client and service - it makes the HTTP connection, serializes the Review object to JSON/etc. so that the remote service can process it.

## External projects using Activout.RestClient

- [Activout.FuelPrice](https://github.com/twogood/Activout.FuelPrice) A console application that reads from Twitter of a specific chain of petrol stations to fetch my local fuel price.
- Your project here?

## Usage notes

- Built for ASP.NET Core 3.1
- Both synchronous and asynchronous calls are supported. Asynchronous is recommended.
- Additional serializers and deserializers can be added at will.
- Support for custom error objects via \[ErrorResponse\] attribute. These will be included in a RestClientException that is thrown if the API call fails.

### Usage with dependency injection through IServiceCollection

```C#
public static IServiceCollection AddRestClient(this IServiceCollection self)
{
  self.TryAddTransient<IDuckTyping, DuckTyping>();
  self.TryAddTransient<IParamConverterManager, ParamConverterManager>();
  self.TryAddTransient<IRestClientFactory, RestClientFactory>();
  self.TryAddTransient<ITaskConverterFactory, TaskConverter2Factory>();
  return self;
}
```


## Breaking changes in version 3

- Changed IRestClientBuilder.HttpClient() to a new overload of IRestClientBuilder.With()
- Removed dependency on Microsoft.AspNetCore.Mvc.Core
  - Removed AddRestClient extension method on IServiceCollection, see Usage notes above
  - This means we now use our own attributes instead of those in Microsoft.AspNetCore.Mvc namespace:
    - \[HttpGet] → \[Get]
    - \[HttpPost] → \[Post]
    - \[InterfaceRoute] → \[Route]
    - \[InterfaceConsumes] and \[Consumes] → \[Accept] for setting Accept HTTP header or \[ContentType] for POST/PUT data
    - Other attributes keep the same name but live in the Activout.RestClient namespace
  - This also meant replacing MediaTypeCollection and MediaType from Microsoft.AspNetCore.Mvc.Formatters namespace:
    - We have our own MediaType class now, which is just a value object
    - IDeserializer has a new method CanDeserialize and the SupportedMediaTypes property is removed
    - ISerializer has a new method CanSerialize and the SupportedMediaTypes property is removed
    - ISerializationManager method signatures changed accordingly


## TODO

- Support for cookie parameters, if someone need them
- Maybe extract JSON serialization/deserialization to its own project so that Newtonsoft.Json dependency becomes optional

## Similar projects

I deliberately implemented my project without even searching for C# projects using the same concept, but afterwards I have found these:

- [Nancy.Rest.Client](https://github.com/maxpiva/Nancy.Rest.Client)
- [Rest.ServiceProxy](https://github.com/sirnewton01/Rest.ServiceProxy)

## Collaborate
This project is still under development - participation welcome!

## Related projects

- [Activout.DatabaseClient](https://github.com/twogood/Activout.DatabaseClient/) - Create a database client only by defining methods on an interface and annotate them with SQL statements. Uses Dapper for object mapping.

## About Activout
[Activout AB](http://activout.se) is a software company in Ronneby, Sweden.
