using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Activout.RestClient.Implementation
{
    internal record RestClientContext
    {
        private static readonly Collection<MediaType> JsonMediaTypeCollection = [MediaType.ValueOf("application/json")];

        public ILogger Logger { get; internal set; } = NullLogger.Instance;
        public Uri BaseUri { get; internal set; }
        public string BaseTemplate { get; internal set; } = "";
        public ISerializer DefaultSerializer { get; internal set; }
        public ISerializationManager SerializationManager { get; internal set; }
        public HttpClient HttpClient { get; internal set; }
        public ITaskConverterFactory TaskConverterFactory { get; internal set; }
        public Type ErrorResponseType { get; internal set; } = typeof(string);
        public MediaType DefaultContentType { get; internal set; } = JsonMediaTypeCollection.FirstOrDefault();
        public IParamConverterManager ParamConverterManager { get; internal set; }
        public List<KeyValuePair<string, object>> DefaultHeaders { get; set; } = [];
        public IRequestLogger RequestLogger { get; set; } = new DummyRequestLogger();
        public Type DomainExceptionType { get; set; }
        public IDomainExceptionMapperFactory DomainExceptionMapperFactory { get; set; }
        public bool UseDomainException => DomainExceptionType != null;
    }

    internal sealed class DummyRequestLogger : IRequestLogger, IDisposable
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