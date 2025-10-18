using System.Net;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using Xunit.Abstractions;

namespace Activout.RestClient.Xml.Test;

public class XmlTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/xml";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new MockHttpMessageHandler();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    [Theory]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    public async Task TestGetSingleMovie(string mediaType)
    {
        _mockHttp.When(HttpMethod.Get, $"{BaseUri}/movies/42")
            .Respond(mediaType,
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <movie>
                    <id>42</id>
                    <title>Hitchhiker's Guide to the Galaxy</title>
                    <reviews>
                        <review>
                            <stars>5</stars>
                            <text>Great movie!</text>
                        </review>
                        <review>
                            <stars>1</stars>
                            <text>Lousy movie!</text>
                        </review>
                    </reviews>
                </movie>
                """);

        var xmlClient = CreateXmlMovieClient();
        var movie = await xmlClient.GetMovie(42);

        Assert.Equal(42, movie.Id);
        Assert.Equal("Hitchhiker's Guide to the Galaxy", movie.Title);
        Assert.Equal(2, movie.Reviews.Length);
        Assert.Equal(5, movie.Reviews[0].Stars);
        Assert.Equal("Great movie!", movie.Reviews[0].Text);
        Assert.Equal(1, movie.Reviews[1].Stars);
        Assert.Equal("Lousy movie!", movie.Reviews[1].Text);
    }

    [Fact]
    public async Task TestGetMovies()
    {
        _mockHttp.When(HttpMethod.Get, $"{BaseUri}/movies")
            .Respond("text/xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <movies>
                    <movie>
                        <id>1</id>
                        <title>Movie 1 ÅÄÖ</title>
                        <reviews>
                            <review>
                                <stars>5</stars>
                                <text>Great movie!</text>
                            </review>
                            <review>
                                <stars>1</stars>
                                <text>Lousy movie!</text>
                            </review>
                        </reviews>
                    </movie>
                </movies>
                """);

        var xmlClient = CreateXmlMovieClient();
        var wrapper = await xmlClient.GetMovies();

        Assert.Single(wrapper.Movies);
        var movie = wrapper.Movies[0];
        Assert.Equal(1, movie.Id);
        Assert.Equal("Movie 1 ÅÄÖ", movie.Title);
        Assert.Equal(2, movie.Reviews.Length);
        Assert.Equal(5, movie.Reviews[0].Stars);
        Assert.Equal("Great movie!", movie.Reviews[0].Text);
        Assert.Equal(1, movie.Reviews[1].Stars);
        Assert.Equal("Lousy movie!", movie.Reviews[1].Text);
    }

    [Fact]
    public async Task TestAddReview()
    {
        _mockHttp.When(HttpMethod.Post, $"{BaseUri}/movies/42/reviews")
            .WithPartialContent("<stars>5</stars>")
            .WithPartialContent("<text>Great movie! ÅÄÖ</text>")
            .Respond(HttpStatusCode.NoContent);

        var xmlClient = CreateXmlMovieClient();
        await xmlClient.AddReview(42, new Review { Stars = 5, Text = "Great movie! ÅÄÖ" });
    }

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .WithXml()
            .With(_loggerFactory.CreateLogger<XmlTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri);
    }

    private IXmlMovieClient CreateXmlMovieClient()
    {
        return CreateRestClientBuilder()
            .Build<IXmlMovieClient>();
    }
}