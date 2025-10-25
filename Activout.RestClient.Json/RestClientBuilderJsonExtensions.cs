using System.Text.Json;
using static Activout.RestClient.Json.SystemTextJsonDefaults;

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
    /// <param name="mediaTypes">Optional list of content types to support. If not provided, defaults will be used.</param>
    /// <returns>The REST client builder instance.</returns>
    public static IRestClientBuilder WithSystemTextJson(this IRestClientBuilder builder,
        JsonSerializerOptions? jsonSerializerOptions = null,
        MediaType[]? mediaTypes = null)
    {
        mediaTypes ??= MediaTypes;

        builder.With(new SystemTextJsonSerializer(jsonSerializerOptions, mediaTypes));
        builder.With(new SystemTextJsonDeserializer(jsonSerializerOptions, mediaTypes));
        builder.Accept(string.Join(", ", mediaTypes.Select(type => type.Value)));
        builder.ContentType(mediaTypes.First());

        return builder;
    }
}