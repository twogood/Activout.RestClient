# Activout.RestClient.Xml

This project is an extension to [Activout.RestClient](https://www.nuget.org/packages/Activout.RestClient/). 

It provides support for XML serialization and deserialization.

## Example usage

### Add XML support from this package

```C#
var restClientFactory = Services.CreateRestClientFactory();
var xmlClient = restClientFactory
    .CreateBuilder()
    .WithXml()  // Add XML support
    .BaseUri(BaseUri)
    .Build<IXmlMovieClient>();

var movie = await xmlClient.GetMovie(42);

await xmlClient.AddReview(42, new Review { Stars = 5, Text = "Great movie!" });
```

### Make sure to add XML serialization attributes to the API model classes

Use attributes like `XmlRoot`, `XmlElement`, `XmlArray`, `XmlArrayItem` to control XML serialization and deserialization.

```C#
[XmlRoot("review")]
public class Review
{
    [XmlElement("stars")]
    public int Stars { get; set; }

    [XmlElement("text")]
    public string Text { get; set; }
}

[XmlRoot("movie")]
public class Movie
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("title")]
    public string Title { get; set; } = "";

    [XmlArray("reviews")]
    [XmlArrayItem("review")]
    public Review[] Reviews { get; set; } = [];
}

[XmlRoot("movies")]
public class MoviesWrapper
{
    [XmlElement("movie")] 
    public Movie[] Movies { get; set; } = [];
}

public interface IXmlMovieClient
{
    [Get("movies/{id}")]
    Task<Movie> GetMovie(int id);

    [Get("movies")]
    Task<MoviesWrapper> GetMovies();

    [Post("movies/{id}/reviews")]
    Task AddReview(int id, Review review);
}
```

## Need help implementing Activout.RestClient?

Contact [david@activout.se](mailto:david@activout.se) to order a support package, starting at 20000 SEK or 1800 EUR or 2000 USD.

## License

[MIT License](LICENSE).

## About Activout
[Activout AB](http://activout.se) is a software company in Ronneby, Sweden.
