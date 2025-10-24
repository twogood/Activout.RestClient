using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// Default configuration values for System.Text.Json serialization.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class SystemTextJsonDefaults
{
    /// <summary>
    /// Gets the collection of supported media types.
    /// </summary>
    public static readonly MediaType[] MediaTypes =
    [
        new MediaType("application/json")
    ];

    /// <summary>
    /// Gets the default JSON converters.
    /// </summary>
    public static readonly JsonConverter[] JsonConverters = [new SimpleValueObjectConverter()];

    /// <summary>
    /// Gets the default JSON serializer options.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true
    };

    public static readonly JsonSerializerOptions CamelCaseSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNameCaseInsensitive = true
    };

    static SystemTextJsonDefaults()
    {
        foreach (var converter in JsonConverters)
        {
            SerializerOptions.Converters.Add(converter);
        }
    }
}