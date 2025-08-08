using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Activout.RestClient.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test;

public class TestRequest
{
    public string Name { get; set; } = "TestValue";
    public int Number { get; set; } = 42;
}

public interface ICancellationTokenBodyClient
{
    [Post("/post-with-token")]
    Task PostWithCancellationToken(TestRequest body, CancellationToken cancellationToken);

    [Put("/put-with-token")]
    Task PutWithCancellationToken(TestRequest body, CancellationToken cancellationToken);

    [Patch("/patch-with-token")]
    Task PatchWithCancellationToken(TestRequest body, CancellationToken cancellationToken);

    [Post("/post-without-token")]
    Task PostWithoutCancellationToken(TestRequest body);
}

public class CancellationTokenBodyTest
{
    private const string BaseUri = "https://example.com/";

    private readonly IRestClientFactory _restClientFactory;
    private readonly MockHttpMessageHandler _mockHttp;

    public CancellationTokenBodyTest()
    {
        _restClientFactory = Services.CreateRestClientFactory();
        _mockHttp = new MockHttpMessageHandler();
    }

    private IRestClientBuilder CreateRestClientBuilder()
    {
        return _restClientFactory.CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri));
    }

    private ICancellationTokenBodyClient CreateClient()
    {
        return CreateRestClientBuilder()
            .WithSystemTextJson()
            .Build<ICancellationTokenBodyClient>();
    }

    [Fact]
    public async Task PostWithCancellationToken_ShouldSendBodyNotToken()
    {
        // Arrange
        var client = CreateClient();
        var body = new TestRequest { Name = "Test", Number = 123 };
        var cancellationToken = new CancellationTokenSource().Token;

        _mockHttp
            .Expect(HttpMethod.Post, BaseUri + "post-with-token")
            .WithContent("{\"Name\":\"Test\",\"Number\":123}")
            .Respond(HttpStatusCode.OK);

        // Act
        await client.PostWithCancellationToken(body, cancellationToken);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task PutWithCancellationToken_ShouldSendBodyNotToken()
    {
        // Arrange
        var client = CreateClient();
        var body = new TestRequest { Name = "Test", Number = 123 };
        var cancellationToken = new CancellationTokenSource().Token;

        _mockHttp
            .Expect(HttpMethod.Put, BaseUri + "put-with-token")
            .WithContent("{\"Name\":\"Test\",\"Number\":123}")
            .Respond(HttpStatusCode.OK);

        // Act
        await client.PutWithCancellationToken(body, cancellationToken);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task PatchWithCancellationToken_ShouldSendBodyNotToken()
    {
        // Arrange
        var client = CreateClient();
        var body = new TestRequest { Name = "Test", Number = 123 };
        var cancellationToken = new CancellationTokenSource().Token;

        _mockHttp
            .Expect(HttpMethod.Patch, BaseUri + "patch-with-token")
            .WithContent("{\"Name\":\"Test\",\"Number\":123}")
            .Respond(HttpStatusCode.OK);

        // Act
        await client.PatchWithCancellationToken(body, cancellationToken);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task PostWithoutCancellationToken_ShouldStillWork()
    {
        // Arrange
        var client = CreateClient();
        var body = new TestRequest { Name = "Test", Number = 123 };

        _mockHttp
            .Expect(HttpMethod.Post, BaseUri + "post-without-token")
            .WithContent("{\"Name\":\"Test\",\"Number\":123}")
            .Respond(HttpStatusCode.OK);

        // Act
        await client.PostWithoutCancellationToken(body);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}