using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Activout.MovieReviews;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RichardSzalay.MockHttp;
using Xunit;

namespace Activout.RestClient.Test
{
    public class FormData
    {
        public string SomeString { get; set; } = "foo";
        public string Unused { get; set; } = null;
        public int SomeNumber { get; set; } = 42;
        [JsonProperty("another")] public string AnotherString { get; set; } = "bar";
    }

    public interface IFormPostClient
    {
        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("/form")]
        Task PostObject(FormData formData);

        [Consumes("application/x-www-form-urlencoded")]
        [HttpPost("/form")]
        Task PostEnumerable(IEnumerable<KeyValuePair<string, string>> formData);
    }

    public class BodyArgumentFormPostTest
    {
        private const string BaseUri = "http://example.com/";

        private readonly IRestClientFactory _restClientFactory;
        private readonly MockHttpMessageHandler _mockHttp;

        public BodyArgumentFormPostTest()
        {
            _restClientFactory = Services.CreateRestClientFactory();
            _mockHttp = new MockHttpMessageHandler();
        }

        private IRestClientBuilder CreateRestClientBuilder()
        {
            return _restClientFactory.CreateBuilder()
                .HttpClient(_mockHttp.ToHttpClient())
                .BaseUri(new Uri(BaseUri));
        }

        private IFormPostClient CreateClient()
        {
            return CreateRestClientBuilder()
                .Build<IFormPostClient>();
        }

        [Fact]
        public async Task TestFormDataObject()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "form")
                .WithFormData("SomeString", "foo")
                .WithFormData("SomeNumber", "42")
                .WithFormData("another", "bar")
                .Respond(HttpStatusCode.OK);

            // Act
            await client.PostObject(new FormData());

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task TestFormDataEnumerable()
        {
            // Arrange
            var client = CreateClient();

            _mockHttp
                .Expect(HttpMethod.Post, BaseUri + "form")
                .WithFormData("SomeString", "foo")
                .WithFormData("SomeNumber", "42")
                .Respond(HttpStatusCode.OK);

            // Act
            await client.PostEnumerable(new[]
            {
                new KeyValuePair<string, string>("SomeString", "foo"),
                new KeyValuePair<string, string>("SomeNumber", "42"),
            });

            // Assert
            _mockHttp.VerifyNoOutstandingExpectation();
        }
    }
}