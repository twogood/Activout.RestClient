using Newtonsoft.Json;
using static Activout.RestClient.Newtonsoft.Json.NewtonsoftJsonDefaults;

namespace Activout.RestClient.Newtonsoft.Json;

public static class RestClientBuilderNewtonsoftJsonExtensions
{
    public static IRestClientBuilder WithNewtonsoftJson(this IRestClientBuilder builder,
        JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var settings = jsonSerializerSettings ?? DefaultJsonSerializerSettings;

        builder.With(new NewtonsoftJsonSerializer(settings));
        builder.With(new NewtonsoftJsonDeserializer(settings));
        builder.Accept(string.Join(", ", SupportedMediaTypes.Select(type => type.Value)));
        builder.ContentType(SupportedMediaTypes.First());

        return builder;
    }
}