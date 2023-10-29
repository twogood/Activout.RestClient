using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test;

public class MySimpleValueObject
{
    public string Value { get; }

    public MySimpleValueObject(string value)
    {
        Value = value;
    }
}

public class Stuff
{
    public MySimpleValueObject FooBar { get; set; }

    public int? NullableInteger { get; set; }
}

public interface IValueObjectClient
{
    Task<Stuff> GetStuff();

    [Post]
    Task SetStuff(Stuff wrapper);
}

public class SimpleValueObjectTest
{
    private const string BaseUri = "http://example.com/api/";

    private readonly IRestClientFactory _restClientFactory = Services.CreateRestClientFactory();
    private readonly MockHttpMessageHandler _mockHttp = new();

    [Fact]
    public async Task TestSimpleValueObjectSerialization()
    {
        // Arrange
        _mockHttp
            .Expect(HttpMethod.Post, BaseUri)
            .WithContent(JsonConvert.SerializeObject(new
            {
                FooBar = "foobar",
                NullableInteger = 42
            }))
            .Respond(HttpStatusCode.OK);

        var client = CreateClient();

        var wrapper = new Stuff
        {
            FooBar = new MySimpleValueObject("foobar"),
            NullableInteger = 42
        };

        // Act
        await client.SetStuff(wrapper);

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task TestSimpleValueObjectDeserialization()
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(new StringContent(JsonConvert.SerializeObject(new
                {
                    FooBar = "foobar",
                    NullableInteger = 42
                }),
                Encoding.UTF8,
                "application/json"));

        var client = CreateClient();

        // Act
        var result = await client.GetStuff();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Equal("foobar", result.FooBar.Value);
    }

    [Fact]
    public async Task TestSimpleValueObjectDeserializationWithNulls()
    {
        // Arrange
        _mockHttp
            .Expect(BaseUri)
            .Respond(new StringContent(JsonConvert.SerializeObject(new
                {
                    FooBar = (string)null,
                    NullableInteger = (int?)null
                }),
                Encoding.UTF8,
                "application/json"));

        var client = CreateClient();

        // Act
        var result = await client.GetStuff();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
        Assert.Null(result.FooBar);
        Assert.Null(result.NullableInteger);
    }

    private IValueObjectClient CreateClient()
    {
        return _restClientFactory.CreateBuilder()
            .With(_mockHttp.ToHttpClient())
            .BaseUri(new Uri(BaseUri))
            .Build<IValueObjectClient>();
    }
}