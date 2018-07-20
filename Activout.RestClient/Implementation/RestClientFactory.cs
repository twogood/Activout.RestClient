using System.Net.Http;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Implementation
{
    internal class RestClientFactory : IRestClientFactory
    {
        private readonly IDuckTyping _duckTyping;
        private readonly ISerializationManager _serializationManager;
        private readonly IParamConverterManager _paramConverterManager;
        private readonly ITaskConverterFactory _taskConverterFactory;

        public RestClientFactory(
            IDuckTyping duckTyping,
            ISerializationManager serializationManager,
            IParamConverterManager paramConverterManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _duckTyping = duckTyping;
            _serializationManager = serializationManager;
            _paramConverterManager = paramConverterManager;
            _taskConverterFactory = taskConverterFactory;
        }

        public IRestClientBuilder CreateBuilder()
        {
            return new RestClientBuilder(_duckTyping, _serializationManager,
                _paramConverterManager, _taskConverterFactory);
        }
    }
}