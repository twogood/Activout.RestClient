#nullable disable
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.Test.MovieReviews;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Activout.RestClient.Test;

public class NonJsonRestClientTests(ITestOutputHelper outputHelper)
{
    private const string BaseUri = "https://example.com/movieReviewService";
    private const string MovieId = "*MOVIE_ID*";
    private const string ReviewId = "*REVIEW_ID*";

    private readonly IRestClientFactory _restClientFactory = new RestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new MockHttpMessageHandler();
    private readonly ILoggerFactory _loggerFactory = LoggerFactoryHelpers.CreateLoggerFactory(outputHelper);

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_loggerFactory.CreateLogger<NonJsonRestClientTests>())
            .With(_mockHttp.ToHttpClient())
            .BaseUri(BaseUri);
    }

    private IMovieReviewService CreateMovieReviewService()
    {
        return CreateRestClientBuilder()
            .Build<IMovieReviewService>();
    }

    [Fact]
    public async Task TestTimeoutAsync()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/string")
            .Respond(async () =>
            {
                await Task.Delay(1000);
                return new HttpResponseMessage();
            });

        var httpClient = _mockHttp.ToHttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(1);
        var reviewSvc = _restClientFactory.CreateBuilder()
            .With(httpClient)
            .BaseUri(new Uri(BaseUri))
            .Build<IMovieReviewService>();

        // act
        await Assert.ThrowsAsync<TaskCanceledException>(() => reviewSvc.GetString());

        // assert
    }

    [Fact]
    public async Task TestCancellationAsync()
    {
        // arrange
        _mockHttp.When($"{BaseUri}/movies/string")
            .Respond(_ => new HttpResponseMessage());

        var reviewSvc = CreateMovieReviewService();
        var cancellationTokenSource = new CancellationTokenSource();

        // act
        await cancellationTokenSource.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            reviewSvc.GetStringCancellable(cancellationTokenSource.Token));

        // assert
    }

    [Fact]
    public async Task TestNoCancellationAsync()
    {
        // arrange
        _mockHttp.When($"{BaseUri}/movies/string")
            .Respond("text/plain", "test string");

        var reviewSvc = CreateMovieReviewService();

        // act
        var result = await reviewSvc.GetStringCancellable(default);

        // assert
        Assert.Equal("test string", result);
    }

    [Fact]
    public async Task TestRequestLogger()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/string")
            .Respond("text/plain", "test");

        var requestLoggerMock = new Mock<IRequestLogger>();
        requestLoggerMock.Setup(x => x.TimeOperation(It.IsAny<HttpRequestMessage>()))
            .Returns(() => new Mock<IDisposable>().Object);

        var reviewSvc = CreateRestClientBuilder()
            .With(requestLoggerMock.Object)
            .Build<IMovieReviewService>();

        // act
        await reviewSvc.GetString();
        await reviewSvc.GetString();

        // assert
        requestLoggerMock.Verify(x => x.TimeOperation(It.IsAny<HttpRequestMessage>()), Times.Exactly(2));
    }

    [Fact]
    public void TestErrorEmptyNoContentType()
    {
        // arrange
        _mockHttp
            .When(HttpMethod.Get, $"{BaseUri}/movies/fail")
            .Respond(HttpStatusCode.BadRequest, _ => new ByteArrayContent(new byte[0]));

        var reviewSvc = CreateMovieReviewService();

        // act
        var aggregateException = Assert.Throws<AggregateException>(() => reviewSvc.Fail());

        // assert
        var exception = (RestClientException)aggregateException.GetBaseException();
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        Assert.NotNull(exception.ErrorResponse);
        Assert.IsType<byte[]>(exception.ErrorResponse);
        Assert.Empty(exception.GetErrorResponse<byte[]>()!);
    }

    [Fact]
    public void TestDelete()
    {
        // arrange
        _mockHttp
            .Expect(HttpMethod.Delete, $"{BaseUri}/movies/{MovieId}/reviews/{ReviewId}")
            .Respond(HttpStatusCode.OK);

        var reviewSvc = CreateMovieReviewService();

        // act
        reviewSvc.DeleteReview(MovieId, ReviewId);

        // assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestPostTextAsync()
    {
        // arrange
        _mockHttp
            .When(HttpMethod.Post, $"{BaseUri}/movies/import.csv")
            .WithContent("foobar")
            .WithHeaders("Content-Type", "text/csv; charset=utf-8")
            .Respond(HttpStatusCode.NoContent);

        var reviewSvc = CreateMovieReviewService();

        // act
        await reviewSvc.Import("foobar");

        // assert
    }

    [Fact]
    public async Task TestGetHttpContent()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies")
            .Respond("text/plain", "test content");

        var reviewSvc = CreateMovieReviewService();

        // act
        var httpContent = reviewSvc.GetHttpContent();

        // assert
        Assert.Equal("test content", await httpContent.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestGetHttpResponseMessage()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies")
            .Respond("text/plain", "test content");

        var reviewSvc = CreateMovieReviewService();

        // act
        var httpResponseMessage = reviewSvc.GetHttpResponseMessage();

        // assert
        Assert.Equal("test content", await httpResponseMessage.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TestGetByteArray()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/bytes")
            .Respond(new ByteArrayContent(new byte[] { 42 }));

        var reviewSvc = CreateMovieReviewService();

        // act
        var bytes = await reviewSvc.GetByteArray();

        // assert
        Assert.Equal(new byte[] { 42 }, bytes);
    }

    [Fact]
    public async Task TestGetEmptyByteArray()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/bytes")
            .Respond(new ByteArrayContent(new byte[0]));

        var reviewSvc = CreateMovieReviewService();

        // act
        var bytes = await reviewSvc.GetByteArray();

        // assert
        Assert.NotNull(bytes);
        Assert.Empty(bytes);
    }

    [Fact]
    public async Task TestGetByteArrayObject()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/byte-object")
            .Respond(new ByteArrayContent(new byte[] { 42 }));

        var reviewSvc = CreateMovieReviewService();

        // act
        var byteArrayObject = await reviewSvc.GetByteArrayObject();

        // assert
        Assert.Equal(new byte[] { 42 }, byteArrayObject.Bytes);
    }

    [Fact]
    public async Task TestGetByteArrayObjectWithEmptyArray()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/byte-object")
            .Respond(new ByteArrayContent(new byte[0]));

        var reviewSvc = CreateMovieReviewService();

        // act
        var byteArrayObject = await reviewSvc.GetByteArrayObject();

        // assert
        Assert.Null(byteArrayObject);
    }

    [Fact]
    public async Task TestGetString()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/string")
            .WithHeaders("accept", "text/plain")
            .Respond(new StringContent("foo"));

        var reviewSvc = CreateMovieReviewService();

        // act
        var result = await reviewSvc.GetString();

        // assert
        Assert.Equal("foo", result);
    }

    [Fact]
    public async Task TestGetStringObject()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/string-object")
            .WithHeaders("accept", "text/plain")
            .Respond(new StringContent("bar"));

        var reviewSvc = CreateMovieReviewService();

        // act
        var stringObject = await reviewSvc.GetStringObject();

        // assert
        Assert.Equal("bar", stringObject.Value);
    }

    [Fact]
    public async Task TestFormPost()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/form")
            .WithFormData("value", "foobar")
            .Respond("text/plain", "");

        var reviewSvc = CreateMovieReviewService();

        // act
        await reviewSvc.FormPost("foobar");

        // assert
    }

    [Fact]
    public async Task TestHeaderParam()
    {
        // arrange
        _mockHttp
            .When($"{BaseUri}/movies/headers")
            .WithHeaders("X-Foo", "bar")
            .Respond("text/plain", "");

        var reviewSvc = CreateRestClientBuilder()
            .Header(new AuthenticationHeaderValue("Basic", "SECRET"))
            .Header("X-Tick", new TickValue())
            .Build<IMovieReviewService>();

        // act
        var responseMessage1 = await reviewSvc.SendFooHeader("bar");
        var requestHeaders1 = responseMessage1.RequestMessage?.Headers;

        var responseMessage2 = await reviewSvc.SendFooHeader("bar");
        var requestHeaders2 = responseMessage2.RequestMessage?.Headers;

        // assert
        Assert.NotNull(requestHeaders1?.Authorization);
        Assert.Equal("Basic SECRET", requestHeaders1.Authorization.ToString());
        Assert.NotEmpty(requestHeaders1.GetValues("X-Tick"));

        Assert.NotEqual(
            requestHeaders1.GetValues("X-Tick").First(),
            requestHeaders2?.GetValues("X-Tick").First());
    }
}

internal class TickValue
{
    private long _ticks = 42;

    public override string ToString()
    {
        return Interlocked.Increment(ref _ticks).ToString();
    }
}