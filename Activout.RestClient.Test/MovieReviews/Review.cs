namespace Activout.RestClient.Test.MovieReviews;

public class Review
{
    public Review(int stars, string text)
    {
        Stars = stars;
        Text = text;
    }

    public string MovieId { get; set; }
    public string ReviewId { get; set; }
    public int Stars { get; set; }
    public string Text { get; set; }
}