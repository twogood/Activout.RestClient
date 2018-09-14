using Activout.RestClient.Helpers;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Implementation;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.ParamConverter.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Activout.RestClient
{
    public static class Services
    {
        public static IServiceCollection AddRestClient(this IServiceCollection self)
        {
            self.TryAddTransient<IDuckTyping, DuckTyping>();
            self.TryAddTransient<IParamConverterManager, ParamConverterManager>();
            self.TryAddTransient<IRestClientFactory, RestClientFactory>();
            self.TryAddTransient<ITaskConverterFactory, TaskConverterFactory>();
            return self;
        }

        public static IDuckTyping CreateDuckTyping()
        {
            return new DuckTyping();
        }

        public static ITaskConverterFactory CreateTaskConverterFactory()
        {
            return new TaskConverterFactory();
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