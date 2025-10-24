#nullable disable
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
    internal class RestClientContext
    {
        private static readonly Collection<MediaType> JsonMediaTypeCollection = new Collection<MediaType>
        {
            MediaType.ValueOf("application/json")
        };

        public RestClientContext()
        {
            BaseTemplate = "";
            DefaultContentType = JsonMediaTypeCollection.FirstOrDefault();
            DefaultHeaders = new List<KeyValuePair<string, object>>();
            ErrorResponseType = typeof(string);
        }

        public ILogger Logger { get; internal set; } = NullLogger.Instance;
        public Uri BaseUri { get; internal set; }
        public string BaseTemplate { get; internal set; }
        public ISerializer DefaultSerializer { get; internal set; }
        public ISerializationManager SerializationManager { get; internal set; }
        public HttpClient HttpClient { get; internal set; }
        public ITaskConverterFactory TaskConverterFactory { get; internal set; }
        public Type ErrorResponseType { get; internal set; }
        public MediaType DefaultContentType { get; internal set; }
        public IParamConverterManager ParamConverterManager { get; internal set; }
        public List<KeyValuePair<string, object>> DefaultHeaders { get; }
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