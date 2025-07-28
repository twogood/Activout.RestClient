using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.Test.MovieReviews
{
    [Path("movies")]
    [ErrorResponse(typeof(ErrorResponse))]
    public interface IMovieReviewService
    {
        [Delete("/{movieId}/reviews/{reviewId}")]
        void DeleteReview(string movieId, string reviewId);

        [Get("/fail")]
        [ErrorResponse(typeof(byte[]))]
        void Fail();

        [Post("/import.csv")]
        [ContentType("text/csv")]
        Task Import(string csv);

        HttpContent GetHttpContent();

        HttpResponseMessage GetHttpResponseMessage();

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

        [Post("/form")]
        Task FormPost([FormParam] string value);

        [Path("/headers")]
        Task<HttpResponseMessage> SendFooHeader([HeaderParam("X-Foo")] string foo);
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