using System.Collections.Generic;

namespace Activout.RestClient.Newtonsoft.Json.Test.MovieReviews
{
    public class ErrorResponse
    {
        public List<Error> Errors { get; init; } = [];

        public class Error
        {
            public string? Message { get; init; }
            public int Code { get; init; }
        }
    }
}