using System;
using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Implementation
{
    internal class RestClientBuilder : IRestClientBuilder
    {
        private readonly IDuckTyping duckTyping;
        private readonly HttpClient httpClient;
        private readonly ISerializationManager serializationManager;
        private readonly ITaskConverterFactory taskConverterFactory;
        private Uri baseUri;

        public RestClientBuilder(HttpClient httpClient, IDuckTyping duckTyping,
            ISerializationManager serializationManager, ITaskConverterFactory taskConverterFactory)
        {
            this.httpClient = CheckNotNull(httpClient);
            this.duckTyping = CheckNotNull(duckTyping);
            this.taskConverterFactory = CheckNotNull(taskConverterFactory);
            this.serializationManager = serializationManager;
        }

        public IRestClientBuilder BaseUri(Uri apiUri)
        {
            baseUri = AddTrailingSlash(apiUri);
            return this;
        }

        public T Build<T>() where T : class
        {
            var client = new RestClient<T>(baseUri, httpClient, serializationManager, taskConverterFactory);
            return duckTyping.DuckType<T>(client);
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