using System.Text.Json;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// Implementation of <see cref="IDeserializer"/> that deserializes JSON using System.Text.Json.
/// </summary>
public class SystemTextJsonDeserializer(
    JsonSerializerOptions? jsonSerializerOptions = null,
    MediaType[]? supportedMediaTypes = null)
    : IDeserializer
{
    private readonly JsonSerializerOptions _serializerOptions =
        jsonSerializerOptions ?? SystemTextJsonDefaults.SerializerOptions;

    private readonly MediaType[] _supportedMediaTypes = supportedMediaTypes ?? SystemTextJsonDefaults.MediaTypes;

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
    public async Task<object?> Deserialize(HttpContent content, Type type)
    {
        await using var stream = await content.ReadAsStreamAsync();

        // ReSharper disable once MergeIntoPattern
        if (stream.CanSeek && stream.Length == 0)
        {
            return null;
        }

        return await JsonSerializer.DeserializeAsync(stream, type, _serializerOptions);
    }

    /// <summary>
    /// Determines whether this deserializer can deserialize the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns><c>true</c> if this deserializer can deserialize the specified media type; otherwise, <c>false</c>.</returns>
    public bool CanDeserialize(MediaType mediaType)
    {
        return _supportedMediaTypes.Contains(mediaType);
    }
}