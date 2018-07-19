using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Implementation
{
    internal class RestClientFactory : IRestClientFactory
    {
        private readonly IDuckTyping duckTyping;
        private readonly ISerializationManager serializationManager;
        private readonly ITaskConverterFactory taskConverterFactory;

        public RestClientFactory(IDuckTyping duckTyping, ISerializationManager serializationManager,
            ITaskConverterFactory taskConverterFactory)
        {
            this.duckTyping = duckTyping;
            this.serializationManager = serializationManager;
            this.taskConverterFactory = taskConverterFactory;
        }

        public IRestClientBuilder CreateBuilder(HttpClient httpClient = null)
        {
            return new RestClientBuilder(httpClient, duckTyping, serializationManager, taskConverterFactory);
        }
    }
}