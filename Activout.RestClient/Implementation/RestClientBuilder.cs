#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Activout.RestClient.Serialization.Implementation;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient.Implementation
{
    internal class RestClientBuilder : IRestClientBuilder
    {
        private readonly IDuckTyping _duckTyping;
        private readonly RestClientContext _context;
        private readonly List<ISerializer> _serializers = SerializationManager.DefaultSerializers.ToList();
        private readonly List<IDeserializer> _deserializers = SerializationManager.DefaultDeserializers.ToList();

        public RestClientBuilder(
            IDuckTyping duckTyping,
            IParamConverterManager paramConverterManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _duckTyping = duckTyping ?? throw new ArgumentNullException(nameof(duckTyping));

            _context = new RestClientContext
            {
                TaskConverterFactory = taskConverterFactory ??
                                       throw new ArgumentNullException(nameof(taskConverterFactory)),
                ParamConverterManager = paramConverterManager ??
                                        throw new ArgumentNullException(nameof(paramConverterManager))
            };
        }


        public IRestClientBuilder BaseUri(Uri apiUri)
        {
            _context.BaseUri = AddTrailingSlash(apiUri);
            return this;
        }

        public IRestClientBuilder ContentType(MediaType contentType)
        {
            _context.DefaultContentType = contentType;
            return this;
        }

        public IRestClientBuilder Header(string name, object value)
        {
            _context.DefaultHeaders.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        public IRestClientBuilder With(ILogger logger)
        {
            _context.Logger = logger;
            return this;
        }

        public IRestClientBuilder With(HttpClient httpClient)
        {
            _context.HttpClient = httpClient;
            return this;
        }

        public IRestClientBuilder With(IRequestLogger requestLogger)
        {
            _context.RequestLogger = requestLogger;
            return this;
        }

        public IRestClientBuilder With(IDeserializer deserializer)
        {
            _deserializers.Add(deserializer);
            return this;
        }

        public IRestClientBuilder With(ISerializer serializer)
        {
            _serializers.Add(serializer);
            return this;
        }

        public IRestClientBuilder With(ISerializationManager serializationManager)
        {
            _context.SerializationManager = serializationManager;
            return this;
        }

        public IRestClientBuilder With(ITaskConverterFactory taskConverterFactory)
        {
            _context.TaskConverterFactory = taskConverterFactory;
            return this;
        }

        public IRestClientBuilder With(IDomainExceptionMapperFactory domainExceptionMapperFactory)
        {
            _context.DomainExceptionMapperFactory = domainExceptionMapperFactory;
            return this;
        }

        public T Build<T>() where T : class
        {
            if (_context.HttpClient == null)
            {
                _context.HttpClient = new HttpClient();
            }

            if (_context.SerializationManager == null)
            {
                _context.SerializationManager = new SerializationManager(_serializers, _deserializers);
            }

            if (_context.DomainExceptionMapperFactory == null)
            {
                _context.DomainExceptionMapperFactory = new DefaultDomainExceptionMapperFactory();
            }

            var client = new RestClient<T>(_context);
            return _duckTyping.DuckType<T>(client);
        }

        private static Uri AddTrailingSlash(Uri apiUri)
        {
            var uriBuilder = new UriBuilder(apiUri ?? throw new ArgumentNullException(nameof(apiUri)));
            if (uriBuilder.Path.EndsWith("/"))
            {
                return apiUri;
            }

            uriBuilder.Path = uriBuilder.Path + "/";
            return uriBuilder.Uri;
        }
    }
}