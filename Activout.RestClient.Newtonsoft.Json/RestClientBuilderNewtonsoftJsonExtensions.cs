using Activout.RestClient.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Activout.RestClient.Newtonsoft.Json
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class RestClientBuilderNewtonsoftJsonExtensions
    {
        public static readonly IReadOnlyCollection<JsonConverter> DefaultJsonConverters = new List<JsonConverter>
            { new SimpleValueObjectConverter() }.ToImmutableList();

        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
        {
            Converters = DefaultJsonConverters.ToList()
        };

        public static IRestClientBuilder WithNewtonsoftJson(this IRestClientBuilder builder,
            JsonSerializerSettings jsonSerializerSettings = null)
        {
            var settings = jsonSerializerSettings ?? DefaultJsonSerializerSettings;

            builder.With(new NewtonsoftJsonSerializer(settings));
            builder.With(new NewtonsoftJsonDeserializer(settings));

            return builder;
        }
    }
}