using Activout.RestClient.Helpers;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Implementation;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.ParamConverter.Implementation;

namespace Activout.RestClient
{
    public static class Services
    {
        public static IDuckTyping CreateDuckTyping()
        {
            return new DuckTyping();
        }

        public static ITaskConverterFactory CreateTaskConverterFactory()
        {
            return new TaskConverter3Factory();
        }

        public static IRestClientFactory CreateRestClientFactory(
            IDuckTyping duckTyping = null,
            ParamConverterManager paramConverterManager = null,
            ITaskConverterFactory taskConverterFactory = null)
        {
            return new RestClientFactory(
                duckTyping ?? CreateDuckTyping(),
                paramConverterManager ?? CreateParamConverterManager(),
                taskConverterFactory ?? CreateTaskConverterFactory());
        }

        public static IParamConverterManager CreateParamConverterManager()
        {
            return new ParamConverterManager();
        }
    }
}