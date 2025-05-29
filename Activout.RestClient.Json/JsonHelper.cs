namespace Activout.RestClient.Json;

/// <summary>
/// Helper class for JSON serialization and deserialization.
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Gets the collection of supported media types.
    /// </summary>
    public static IReadOnlyCollection<MediaType> SupportedMediaTypes { get; } =
    [
        MediaType.ValueOf("application/json")
    ];
}