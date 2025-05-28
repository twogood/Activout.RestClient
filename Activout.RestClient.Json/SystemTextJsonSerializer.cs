using System.Text;
using System.Text.Json;
using Activout.RestClient.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// Implementation of <see cref="ISerializer"/> that serializes objects as JSON
/// using System.Text.Json.
/// </summary>
public class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Gets the collection of supported media types.
    /// </summary>
    public IReadOnlyCollection<MediaType> SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The JSON serializer options.</param>
    public SystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Gets or sets the order of this serializer in the chain of serializers.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Serializes the specified data object to JSON.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <param name="mediaType">The media type.</param>
    /// <returns>The serialized data as <see cref="HttpContent"/>.</returns>
    public HttpContent Serialize(object data, Encoding encoding, MediaType mediaType)
    {
        return new StringContent(
            JsonSerializer.Serialize(data, _jsonSerializerOptions),
            encoding, mediaType.Value);
    }

    /// <summary>
    /// Determines whether this serializer can serialize the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type.</param>
    /// <returns><c>true</c> if this serializer can serialize the specified media type; otherwise, <c>false</c>.</returns>
    public bool CanSerialize(MediaType mediaType)
    {
        return SupportedMediaTypes.Contains(mediaType);
    }
}