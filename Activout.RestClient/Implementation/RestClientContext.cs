using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Implementation
{
    internal class RestClientContext
    {
        private static readonly MediaTypeCollection JsonMediaTypeCollection = new MediaTypeCollection
        {
            "application/json"
        };

        public RestClientContext()
        {
            BaseTemplate = "";
            DefaultContentTypes = JsonMediaTypeCollection;
            DefaultHeaders = new List<KeyValuePair<string, object>>();
            ErrorResponseType = typeof(string);
        }

        public Uri BaseUri { get; internal set; }
        public string BaseTemplate { get; internal set; }
        public ISerializer DefaultSerializer { get; internal set; }
        public ISerializationManager SerializationManager { get; internal set; }
        public HttpClient HttpClient { get; internal set; }
        public ITaskConverterFactory TaskConverterFactory { get; internal set; }
        public Type ErrorResponseType { get; internal set; }
        public MediaTypeCollection DefaultContentTypes { get; internal set; }
        public IParamConverterManager ParamConverterManager { get; internal set; }
        public List<KeyValuePair<string, object>> DefaultHeaders { get; }
        public IRequestLogger RequestLogger { get; set; } = new DummyRequestLogger();
    }

    internal class DummyRequestLogger : IRequestLogger, IDisposable
    {
        public IDisposable TimeOperation(HttpRequestMessage httpRequestMessage)
        {
            return this;
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}