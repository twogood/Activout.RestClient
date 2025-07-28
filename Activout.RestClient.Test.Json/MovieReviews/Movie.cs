using System.Diagnostics.CodeAnalysis;

namespace Activout.RestClient.Test.Json.MovieReviews;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Movie
{
    public string Title { get; set; } = string.Empty;
}