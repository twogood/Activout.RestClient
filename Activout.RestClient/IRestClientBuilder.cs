using System;
using System.Net.Http;
using Activout.RestClient.DomainExceptions;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;
using Microsoft.Extensions.Logging;

namespace Activout.RestClient;

public interface IRestClientBuilder
{
    IRestClientBuilder BaseUri(Uri apiUri);
    IRestClientBuilder ContentType(MediaType contentType);
    IRestClientBuilder Header(string name, object value, bool isReplace = true);
    IRestClientBuilder With(IDuckTyping duckTyping);
    IRestClientBuilder With(ILogger logger);
    IRestClientBuilder With(HttpClient httpClient);
    IRestClientBuilder With(IRequestLogger requestLogger);
    IRestClientBuilder With(IDeserializer deserializer);
    IRestClientBuilder With(ISerializer serializer);
    IRestClientBuilder With(ISerializationManager serializationManager);
    IRestClientBuilder With(ITaskConverterFactory taskConverterFactory);
    IRestClientBuilder With(IDomainExceptionMapperFactory domainExceptionMapperFactory);
    IRestClientBuilder With(IParamConverterManager paramConverterManager);
    T Build<T>() where T : class;
}