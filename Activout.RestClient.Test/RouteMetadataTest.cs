using System;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.RestClient.Implementation;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test;

public class RouteMetadataTest
{
    [Path("items")]
    public interface IItemClient
    {
        [Get("/{id}")]
        [Header("X-Method", "m")]
        Task<string> Get([PathParam] int id);
    }

    [Fact]
    public void Get_SameMethod_ReturnsCachedInstance()
    {
        var method = typeof(IItemClient).GetMethod(nameof(IItemClient.Get))!;

        Assert.Same(RouteMetadata.Get(method), RouteMetadata.Get(method));
        Assert.Equal("/{id}", RouteMetadata.Get(method).TemplateSuffix);
    }

    [Fact]
    public async Task TwoClients_ShareMetadata_KeepOwnContext()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.Expect(HttpMethod.Get, "https://a.example.com/items/1")
            .WithHeaders("X-Client", "a").WithHeaders("X-Method", "m")
            .Respond("text/plain", "a");
        mockHttp.Expect(HttpMethod.Get, "https://b.example.com/items/2")
            .WithHeaders("X-Client", "b").WithHeaders("X-Method", "m")
            .Respond("text/plain", "b");

        IItemClient Build(string name) => new RestClientFactory().CreateBuilder()
            .With(mockHttp.ToHttpClient())
            .BaseUri(new Uri($"https://{name}.example.com"))
            .Header("X-Client", name)
            .Build<IItemClient>();

        Assert.Equal("a", await Build("a").Get(1));
        Assert.Equal("b", await Build("b").Get(2));
        mockHttp.VerifyNoOutstandingExpectation();
    }
}
