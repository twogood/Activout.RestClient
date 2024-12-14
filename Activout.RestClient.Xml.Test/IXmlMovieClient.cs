using System.Xml.Serialization;

namespace Activout.RestClient.Xml.Test;

[XmlRoot("review")]
public class Review
{
    [XmlElement("stars")] public int Stars { get; set; }
    [XmlElement("text")] public string Text { get; set; } = "";
}

[XmlRoot("movie")]
public class Movie
{
    [XmlElement("id")] public int Id { get; set; }
    [XmlElement("title")] public string Title { get; set; } = "";

    [XmlArray("reviews")]
    [XmlArrayItem("review")]
    public Review[] Reviews { get; set; } = [];
}

[XmlRoot("movies")]
public class MoviesWrapper
{
    [XmlElement("movie")] public Movie[] Movies { get; set; } = [];
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