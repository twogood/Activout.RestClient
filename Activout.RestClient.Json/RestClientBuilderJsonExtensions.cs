using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Activout.RestClient.Json;

/// <summary>
/// Extension methods for configuring RestClient with System.Text.Json support.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class RestClientBuilderJsonExtensions
{
    /// <summary>
    /// Gets the default JSON converters.
    /// </summary>
    public static readonly IReadOnlyCollection<System.Text.Json.Serialization.JsonConverter> DefaultJsonConverters =
        new List<System.Text.Json.Serialization.JsonConverter> { new SimpleValueObjectConverter() }.ToImmutableList();

    /// <summary>
    /// Gets the default JSON serializer options.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    static RestClientBuilderJsonExtensions()
    {
        foreach (var converter in DefaultJsonConverters)
        {
            DefaultJsonSerializerOptions.Converters.Add(converter);
        }
    }

    /// <summary>
    /// Configures the RestClient to use System.Text.Json for JSON serialization and deserialization.
    /// </summary>
    /// <param name="builder">The REST client builder instance.</param>
    /// <param name="jsonSerializerOptions">Optional custom JSON serializer options. If not provided, default options will be used.</param>
    /// <returns>The REST client builder instance.</returns>
    public static IRestClientBuilder WithSystemTextJson(this IRestClientBuilder builder, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var options = jsonSerializerOptions ?? DefaultJsonSerializerOptions;

        // Register the serializer and deserializer
        builder.With(new SystemTextJsonSerializer(options));
        builder.With(new SystemTextJsonDeserializer(options));

        return builder;
    }
}