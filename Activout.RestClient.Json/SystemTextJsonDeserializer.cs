using System.Text.Json;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// Implementation of <see cref="IDeserializer"/> that deserializes JSON using System.Text.Json.
/// </summary>
public class SystemTextJsonDeserializer : IDeserializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Gets the collection of supported media types.
    /// </summary>
    public IReadOnlyCollection<MediaType> SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonDeserializer"/> class with the specified options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    public SystemTextJsonDeserializer(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Gets or sets the order of this deserializer in the chain of deserializers.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Deserializes the specified HTTP content to the specified type.
    /// </summary>
    /// <param name="content">The HTTP content to deserialize.</param>
    /// <param name="type">The target type.</param>
    /// <returns>The deserialized object.</returns>
    public async Task<object> Deserialize(HttpContent content, Type type)
    {
        var json = await content.ReadAsStringAsync();
            
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return System.Text.Json.JsonSerializer.Deserialize(json, type, _jsonSerializerOptions);
    }

    /// <summary>
    /// Determines whether this deserializer can deserialize the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns><c>true</c> if this deserializer can deserialize the specified media type; otherwise, <c>false</c>.</returns>
    public bool CanDeserialize(MediaType mediaType)
    {
        return SupportedMediaTypes.Contains(mediaType);
    }
}