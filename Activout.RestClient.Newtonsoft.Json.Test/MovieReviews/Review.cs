namespace Activout.RestClient.Newtonsoft.Json.Test.MovieReviews
{
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
}