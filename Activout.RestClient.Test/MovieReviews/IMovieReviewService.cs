using Activout.RestClient;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Activout.MovieReviews
{
    [InterfaceRoute("movies")]
    [ErrorResponse(typeof(ErrorResponse))]
    [InterfaceConsumes("application/json")]
    public interface IMovieReviewService
    {
        [HttpGet]
        Task<IEnumerable<Movie>> GetAllMovies();

        [HttpGet]
        [Route("/{movieId}/reviews")]
        Task<IEnumerable<Review>> GetAllReviews([RouteParam("movieId")] string movieId);

        [HttpGet("/{movieId}/reviews/{reviewId}")]
        Review GetReview([RouteParam("movieId")] string movieId, [RouteParam("reviewId")] string reviewId);

        [HttpPost]
        [Route("/{movieId}/reviews")]
        Task<Review> SubmitReview([RouteParam("movieId")] string movieId, Review review);

        [HttpPut]
        [Route("/{movieId}/reviews/{reviewId}")]
        Review UpdateReview([RouteParam("movieId")] string movieId, [RouteParam("reviewId")] string reviewId, Review review);

        [HttpPost("/import.csv")]
        [Consumes("text/csv")]
        Task Import(string csv);
    }

}
