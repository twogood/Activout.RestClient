# Activout.RestClient.Newtonsoft.Json

This project is an extension to [Activout.RestClient](https://www.nuget.org/packages/Activout.RestClient/). 

It provides support for JSON serialization and deserialization via Newtonsoft.Json.

See the [Activout.RestClient README](https://github.com/twogood/Activout.RestClient/tree/main) for more information.

## Example usage

```C#
var restClientFactory = Services.CreateRestClientFactory();
var movieReviewService = restClientFactory
            .CreateBuilder()
            .With(_httpClient)
            .WithNewtonsoftJson()   // Use this package to enable Newtonsoft.Json serialization
            .BaseUri(new Uri("https://example.com/movieReviewService"))
            .Build<IMovieReviewService>();

var review = new Review(stars: 3, "This was a delightful comedy, but not terribly realistic.");
await movieReviewService.SubmitReview(movieId, review);
```

## Need help implementing Activout.RestClient?

Contact [david@activout.se](mailto:david@activout.se) to order a support package.

## License

[MIT License](LICENSE).

## About Activout
[Activout AB](http://activout.se) is a software company in Ronneby, Sweden.
