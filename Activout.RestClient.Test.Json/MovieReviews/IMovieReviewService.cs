namespace Activout.RestClient.Test.Json.MovieReviews;

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

    [Post]
    [Path("/{movieId}/reviews")]
    Task<Review> SubmitReview([PathParam("movieId")] string movieId, Review review);

    [Put]
    [Path("/{movieId}/reviews/{reviewId}")]
    Review UpdateReview(string movieId, [PathParam] string reviewId, Review review);

    [Patch]
    [Path("/{movieId}/reviews/{reviewId}")]
    Review PartialUpdateReview(string movieId, [PathParam] string reviewId, Review review);

    [Get]
    Task<IEnumerable<Movie>> QueryMoviesByDate(
        [QueryParam] DateTime begin,
        [QueryParam] DateTime end);
}