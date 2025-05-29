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
    /// Gets the default JSON converters.
    /// </summary>
    public static readonly JsonConverter[] DefaultJsonConverters = [new SimpleValueObjectConverter()];

    /// <summary>
    /// Gets the default JSON serializer options.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    static SystemTextJsonDefaults()
    {
        foreach (var converter in DefaultJsonConverters)
        {
            DefaultJsonSerializerOptions.Converters.Add(converter);
        }
    }
}