 namespace Activout.RestClient.Json;

  /// <summary>
  /// Helper class for JSON serialization and deserialization.
  /// </summary>
  public static class JsonHelper
  {
      static JsonHelper()
      {
          SupportedMediaTypes = new[]
          {
              MediaType.ValueOf("application/json")
          };
      }

      /// <summary>
      /// Gets the collection of supported media types.
      /// </summary>
      public static IReadOnlyCollection<MediaType> SupportedMediaTypes { get; }
  }