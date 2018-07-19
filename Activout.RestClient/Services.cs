using Activout.RestClient.Helpers;
using Activout.RestClient.Helpers.Implementation;
using Activout.RestClient.Implementation;
using Activout.RestClient.Serialization;
using Activout.RestClient.Serialization.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Activout.RestClient
{
    public static class Services
    {
        public static IServiceCollection AddRestClient(this IServiceCollection self)
        {
            return self
                    .AddTransient<IDuckTyping, DuckTyping>()
                    .AddTransient<ITaskConverterFactory, TaskConverterFactory>()
                    .AddTransient<IRestClientFactory, RestClientFactory>()
                    .AddTransient<IRestClientBuilder, RestClientBuilder>()
                ;
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
            ISerializationManager serializationManager = null,
            ITaskConverterFactory taskConverterFactory = null)
        {
            return new RestClientFactory(
                duckTyping ?? CreateDuckTyping(),
                serializationManager ?? CreateSerializationManager(),
                taskConverterFactory ?? CreateTaskConverterFactory());
        }

        private static ISerializationManager CreateSerializationManager()
        {
            return new SerializationManager();
        }
    }
}