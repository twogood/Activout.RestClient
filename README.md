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
[InterfaceRoute("movies")]
[ErrorResponse(typeof(ErrorResponse))]
[InterfaceConsumes("application/json")]
public interface IMovieReviewService
{
    Task<List<Movie>> GetAllMovies();

    Task<List<Movie>> QueryMoviesByDate(
        [QueryParam] DateTime begin,
        [QueryParam] DateTime end);

    [HttpGet("/{movieId}/reviews")]
    Task<IEnumerable<Review>> GetAllReviews(string movieId);

    [HttpGet("/{movieId}/reviews/{reviewId}")]
    Task<Review> GetReview(string movieId, string reviewId);

    [HttpPost("/{movieId}/reviews")]
    Task<Review> SubmitReview(string movieId, Review review);

    [HttpPut("/{movieId}/reviews/{reviewId}")]
    Task<Review> UpdateReview(string movieId, string reviewId, Review review);

    [HttpPost("/import.csv")]
    [Consumes("text/csv")]
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

- Exceptions will be wrapped in AggregatedException
- Both synchronous and asynchronous calls are supported. Asynchronous is recommended.
- Additional serializers and deserializers can be added at will.
- Support for custom error objects via \[ErrorResponse\] attribute. These will be included in a RestClientException that is thrown if the API call fails.

## TODO

- Support for cookie parameters
- Support for default headers
- More real-life testing :)

## Collaborate
This project is still under development - participation welcome!
