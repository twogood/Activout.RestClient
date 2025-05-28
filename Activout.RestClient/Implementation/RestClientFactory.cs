using System;
using Activout.RestClient.Helpers;
using Activout.RestClient.ParamConverter;

namespace Activout.RestClient.Implementation
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly IDuckTyping _duckTyping;
        private readonly IParamConverterManager _paramConverterManager;
        private readonly ITaskConverterFactory _taskConverterFactory;

        public RestClientFactory(
            IDuckTyping duckTyping,
            IParamConverterManager paramConverterManager,
            ITaskConverterFactory taskConverterFactory)
        {
            _duckTyping = duckTyping;
            _paramConverterManager = paramConverterManager;
            _taskConverterFactory = taskConverterFactory;
        }

        public IRestClientBuilder CreateBuilder()
        {
            return new RestClientBuilder(_duckTyping, _paramConverterManager, _taskConverterFactory);
        }

        public IRestClientBuilder Extend(IExtendable client)
        {
            if (client.ExtendableContext is not ExtendableContextImpl extendableContext)
            {
                throw new InvalidOperationException("Client must be created by Activout.RestClient");
            }

            return new RestClientBuilder(_duckTyping, extendableContext.Context);
        }
    }
}