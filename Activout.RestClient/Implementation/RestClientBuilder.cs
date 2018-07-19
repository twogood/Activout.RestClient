using System;
using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Implementation
{
    internal class RestClientBuilder : IRestClientBuilder
    {
        private readonly IDuckTyping _duckTyping;
        private readonly HttpClient _httpClient;
        private readonly ISerializationManager _serializationManager;
        private readonly ITaskConverterFactory _taskConverterFactory;
        private Uri _baseUri;

        public RestClientBuilder(HttpClient httpClient, IDuckTyping duckTyping,
            ISerializationManager serializationManager, ITaskConverterFactory taskConverterFactory)
        {
            _httpClient = CheckNotNull(httpClient);
            _duckTyping = CheckNotNull(duckTyping);
            _taskConverterFactory = CheckNotNull(taskConverterFactory);
            _serializationManager = serializationManager;
        }

        public IRestClientBuilder BaseUri(Uri apiUri)
        {
            _baseUri = AddTrailingSlash(apiUri);
            return this;
        }

        public T Build<T>() where T : class
        {
            var client = new RestClient<T>(_baseUri, _httpClient, _serializationManager, _taskConverterFactory);
            return _duckTyping.DuckType<T>(client);
        }

        private static Uri AddTrailingSlash(Uri apiUri)
        {
            var uriBuilder = new UriBuilder(CheckNotNull(apiUri));
            if (uriBuilder.Path.EndsWith("/"))
            {
                return apiUri;
            }

            uriBuilder.Path = uriBuilder.Path + "/";
            return uriBuilder.Uri;
        }
    }
}