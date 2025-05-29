using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Activout.RestClient.Newtonsoft.Json;

public static class RestClientBuilderNewtonsoftJsonExtensions
{
    public static IRestClientBuilder WithNewtonsoftJson(this IRestClientBuilder builder,
        JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var settings = jsonSerializerSettings ?? NewtonsoftJsonDefaults.DefaultJsonSerializerSettings;

        builder.With(new NewtonsoftJsonSerializer(settings));
        builder.With(new NewtonsoftJsonDeserializer(settings));

        return builder;
    }
}