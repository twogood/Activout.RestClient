using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.RestClient;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
        Task<IEnumerable<Review>> GetAllReviews(string movieId);

        [HttpGet("/{movieId}/reviews/{reviewId}")]
        Review GetReview(string movieId, string reviewId);

        [HttpPost]
        [Route("/{movieId}/reviews")]
        Task<Review> SubmitReview([RouteParam("movieId")] string movieId, Review review);

        [HttpPut]
        [Route("/{movieId}/reviews/{reviewId}")]
        Review UpdateReview(string movieId, [RouteParam] string reviewId, Review review);

        [HttpPost("/import.csv")]
        [Consumes("text/csv")]
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
    }
}