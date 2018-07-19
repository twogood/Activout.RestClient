using System;
using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Implementation
{
    internal class RestClientContext
    {
        public Uri BaseUri { get; internal set; }
        public string BaseTemplate { get; internal set; }
        public ISerializer DefaultSerializer { get; internal set; }
        public ISerializationManager SerializationManager { get; internal set; }
        public HttpClient HttpClient { get; internal set; }
        public ITaskConverterFactory TaskConverterFactory { get; internal set; }
        public Type ErrorResponseType { get; internal set; }
        public MediaTypeCollection DefaultContentTypes { get; internal set; }
    }
}