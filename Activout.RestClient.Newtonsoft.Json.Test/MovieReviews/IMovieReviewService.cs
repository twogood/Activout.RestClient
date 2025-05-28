using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Activout.RestClient.Newtonsoft.Json.Test.MovieReviews
{
    [Path("movies")]
    [ErrorResponse(typeof(ErrorResponse))]
    public interface IMovieReviewService
    {
        [Get]
        Task<IEnumerable<Movie>> GetAllMovies();

        [Get]
        Task<IEnumerable<Movie>> GetAllMoviesCancellable(CancellationToken cancellationToken);

        [Get]
        [Path("/{movieId}/reviews")]
        Task<IEnumerable<Review>> GetAllReviews(string movieId);

        [Get("/{movieId}/reviews/{reviewId}")]
        Review GetReview(string movieId, string reviewId);

        [Delete("/{movieId}/reviews/{reviewId}")]
        void DeleteReview(string movieId, string reviewId);

        [Get("/fail")]
        [ErrorResponse(typeof(byte[]))]
        void Fail();

        [Post]
        [Path("/{movieId}/reviews")]
        Task<Review> SubmitReview([PathParam("movieId")] string movieId, Review review);

        [Put]
        [Path("/{movieId}/reviews/{reviewId}")]
        Review UpdateReview(string movieId, [PathParam] string reviewId, Review review);

        [Patch]
        [Path("/{movieId}/reviews/{reviewId}")]
        Review PartialUpdateReview(string movieId, [PathParam] string reviewId, Review review);

        [Post("/import.csv")]
        [ContentType("text/csv")]
        Task Import(string csv);

        [Get]
        Task<IEnumerable<Movie>> QueryMoviesByDate(
            [QueryParam] DateTime begin,
            [QueryParam] DateTime end);

        HttpContent GetHttpContent();

        HttpResponseMessage GetHttpResponseMessage();

        [Path("/object")]
        JObject GetJObject();

        [Path("/array")]
        Task<JArray> GetJArray();

        [Post("/form")]
        Task FormPost([FormParam] string value);

        [Path("/headers")]
        Task<HttpResponseMessage> SendFooHeader([HeaderParam("X-Foo")] string foo);

        [Path("/bytes")]
        Task<byte[]> GetByteArray();

        [Path("/byte-object")]
        Task<ByteArrayObject> GetByteArrayObject();

        [Path("/string")]
        [Accept("text/plain")]
        Task<string> GetString();

        [Path("/string-object")]
        [Accept("text/plain")]
        Task<StringObject> GetStringObject();
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
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