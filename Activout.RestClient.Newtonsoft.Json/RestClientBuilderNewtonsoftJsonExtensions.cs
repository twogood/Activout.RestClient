using Activout.RestClient.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Activout.RestClient.Newtonsoft.Json
{
    public static class RestClientBuilderNewtonsoftJsonExtensions
    {
        public static IRestClientBuilder WithNewtonsoftJson(this IRestClientBuilder builder, JsonSerializerSettings jsonSerializerSettings = null)
        {
            var settings = jsonSerializerSettings ?? new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>(NewtonsoftSerializationManager.DefaultJsonConverters)
            };

            builder.With(new FormUrlEncodedSerializer());
            builder.With(new NewtonsoftJsonSerializer(settings));
            builder.With(new NewtonsoftJsonDeserializer(settings));
            
            return builder;
        }
    }
}
