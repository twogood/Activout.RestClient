using System.Collections.Immutable;
using System.Text.Json;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// Serialization manager that uses System.Text.Json.
/// </summary>
public class JsonSerializationManager : ISerializationManager
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

    static JsonSerializationManager()
    {
        foreach (var converter in DefaultJsonConverters)
        {
            DefaultJsonSerializerOptions.Converters.Add(converter);
        }
    }

    /// <summary>
    /// Gets the default serializers.
    /// </summary>
    public static readonly IReadOnlyCollection<ISerializer> DefaultSerializers = new List<ISerializer>
        {
            new SystemTextJsonSerializer(DefaultJsonSerializerOptions)
        }
        .ToImmutableList();

    /// <summary>
    /// Gets the default deserializers.
    /// </summary>
    public static readonly IReadOnlyCollection<IDeserializer> DefaultDeserializers =
        new List<IDeserializer>
            {
                new SystemTextJsonDeserializer(DefaultJsonSerializerOptions)
            }
            .ToImmutableList();

    private IReadOnlyCollection<ISerializer> Serializers { get; }
    private IReadOnlyCollection<IDeserializer> Deserializers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializationManager"/> class.
    /// </summary>
    /// <param name="serializers">The collection of serializers to use.</param>
    /// <param name="deserializers">The collection of deserializers to use.</param>
    public JsonSerializationManager(IReadOnlyCollection<ISerializer> serializers = null,
        IReadOnlyCollection<IDeserializer> deserializers = null)
    {
        // Combine with core serializers/deserializers if provided
        var allSerializers = new List<ISerializer>();
        if (serializers != null)
        {
            allSerializers.AddRange(serializers);
        }
        allSerializers.AddRange(DefaultSerializers);

        var allDeserializers = new List<IDeserializer>();
        if (deserializers != null)
        {
            allDeserializers.AddRange(deserializers);
        }
        allDeserializers.AddRange(DefaultDeserializers);

        Serializers = allSerializers.OrderBy(s => s.Order).ToArray();
        Deserializers = allDeserializers.OrderBy(s => s.Order).ToArray();
    }

    /// <summary>
    /// Gets a deserializer that can deserialize the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns>A deserializer that can deserialize the specified media type, or <c>null</c> if none is found.</returns>
    public IDeserializer GetDeserializer(MediaType mediaType)
    {
        if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
        return Deserializers.FirstOrDefault(serializer => serializer.CanDeserialize(mediaType));
    }

    /// <summary>
    /// Gets a serializer that can serialize the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns>A serializer that can serialize the specified media type, or <c>null</c> if none is found.</returns>
    public ISerializer GetSerializer(MediaType mediaType)
    {
        if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
        return Serializers.FirstOrDefault(serializer => serializer.CanSerialize(mediaType));
    }
}

