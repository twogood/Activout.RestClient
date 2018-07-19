using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Implementation
{
    internal class RestClientFactory : IRestClientFactory
    {
        private readonly IDuckTyping _duckTyping;
        private readonly ISerializationManager _serializationManager;
        private readonly ITaskConverterFactory _taskConverterFactory;

        public RestClientFactory(IDuckTyping duckTyping, ISerializationManager serializationManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _duckTyping = duckTyping;
            _serializationManager = serializationManager;
            _taskConverterFactory = taskConverterFactory;
        }

        public IRestClientBuilder CreateBuilder(HttpClient httpClient = null)
        {
            return new RestClientBuilder(httpClient, _duckTyping, _serializationManager, _taskConverterFactory);
        }
    }
}