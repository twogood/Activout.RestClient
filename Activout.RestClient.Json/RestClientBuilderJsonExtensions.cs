using System.Text.Json;

namespace Activout.RestClient.Json;

/// <summary>
/// Extension methods for configuring RestClient with System.Text.Json support.
/// </summary>
public static class RestClientBuilderJsonExtensions
{
    /// <summary>
    /// Configures the RestClient to use System.Text.Json for JSON serialization and deserialization.
    /// </summary>
    /// <param name="builder">The REST client builder instance.</param>
    /// <param name="jsonSerializerOptions">Optional custom JSON serializer options. If not provided, default options will be used.</param>
    /// <returns>The REST client builder instance.</returns>
    public static IRestClientBuilder WithSystemTextJson(this IRestClientBuilder builder,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var options = jsonSerializerOptions ?? SystemTextJsonDefaults.DefaultJsonSerializerOptions;

        // Register the serializer and deserializer
        builder.With(new SystemTextJsonSerializer(options));
        builder.With(new SystemTextJsonDeserializer(options));

        return builder;
    }
}