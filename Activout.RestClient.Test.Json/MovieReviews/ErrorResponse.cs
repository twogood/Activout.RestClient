namespace Activout.RestClient.Test.Json.MovieReviews;

public class ErrorResponse
{
    public List<Error> Errors { get; set; } = [];

    public class Error
    {
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
    }
}