using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Implementation
{
    internal class RestClientBuilder : IRestClientBuilder
    {
        private readonly IDuckTyping _duckTyping;
        private readonly RestClientContext _context;

        public RestClientBuilder(
            IDuckTyping duckTyping,
            ISerializationManager serializationManager,
            IParamConverterManager paramConverterManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _duckTyping = CheckNotNull(duckTyping);

            _context = new RestClientContext
            {
                TaskConverterFactory = CheckNotNull(taskConverterFactory),
                SerializationManager = CheckNotNull(serializationManager),
                ParamConverterManager = CheckNotNull(paramConverterManager)
            };
        }

        public IRestClientBuilder BaseUri(Uri apiUri)
        {
            _context.BaseUri = AddTrailingSlash(apiUri);
            return this;
        }

        public IRestClientBuilder HttpClient(HttpClient httpClient)
        {
            _context.HttpClient = httpClient;
            return this;
        }

        public IRestClientBuilder Header(string name, object value)
        {
            _context.DefaultHeaders.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        public IRestClientBuilder Header(AuthenticationHeaderValue authenticationHeaderValue)
        {
            _context.DefaultHeaders.Add(
                new KeyValuePair<string, object>("Authorization", authenticationHeaderValue));
            return this;
        }

        public IRestClientBuilder With(IRequestLogger requestLogger)
        {
            _context.RequestLogger = requestLogger;
            return this;
        }

        public T Build<T>() where T : class
        {
            if (_context.HttpClient == null)
            {
                _context.HttpClient = new HttpClient();
            }

            var client = new RestClient<T>(_context);
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