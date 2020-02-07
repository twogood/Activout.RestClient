using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Activout.MovieReviews;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Test.MovieReviews
{
    [Route("movies")]
    [ErrorResponse(typeof(ErrorResponse))]
    public interface IMovieReviewService
    {
        [HttpGet]
        Task<IEnumerable<Movie>> GetAllMovies();

        [HttpGet]
        Task<IEnumerable<Movie>> GetAllMoviesCancellable(CancellationToken cancellationToken);

        [HttpGet]
        [Route("/{movieId}/reviews")]
        Task<IEnumerable<Review>> GetAllReviews(string movieId);

        [HttpGet("/{movieId}/reviews/{reviewId}")]
        Review GetReview(string movieId, string reviewId);

        [HttpDelete("/{movieId}/reviews/{reviewId}")]
        void DeleteReview(string movieId, string reviewId);

        [HttpGet("/fail")]
        [ErrorResponse(typeof(byte[]))]
        void Fail();

        [HttpPost]
        [Route("/{movieId}/reviews")]
        Task<Review> SubmitReview([RouteParam("movieId")] string movieId, Review review);

        [HttpPut]
        [Route("/{movieId}/reviews/{reviewId}")]
        Review UpdateReview(string movieId, [RouteParam] string reviewId, Review review);

        [HttpPost("/import.csv")]
        [ContentType("text/csv")]
        Task Import(string csv);

        [HttpGet]
        Task<IEnumerable<Movie>> QueryMoviesByDate(
            [QueryParam] DateTime begin,
            [QueryParam] DateTime end);

        HttpContent GetHttpContent();

        HttpResponseMessage GetHttpResponseMessage();

        [Route("/object")]
        JObject GetJObject();

        [Route("/array")]
        Task<JArray> GetJArray();

        [HttpPost("/form")]
        Task FormPost([FormParam] string value);

        [Route("/headers")]
        Task<HttpResponseMessage> SendFooHeader([HeaderParam("X-Foo")] string foo);

        [Route("/bytes")]
        Task<byte[]> GetByteArray();

        [Route("/byte-object")]
        Task<ByteArrayObject> GetByteArrayObject();

        [Route("/string")]
        [Accept("text/plain")]
        Task<string> GetString();

        [Route("/string-object")]
        [Accept("text/plain")]
        Task<StringObject> GetStringObject();
    }

    public class StringObject
    {
        public string Value { get; }

        public StringObject(string value)
        {
            Value = value;
        }
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ByteArrayObject
    {
        public byte[] Bytes { get; }

        public ByteArrayObject(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}