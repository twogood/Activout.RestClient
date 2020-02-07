using System.Collections.Generic;

namespace Activout.RestClient.Serialization.Implementation
{
    public static class JsonHelper
    {
        static JsonHelper()
        {
            SupportedMediaTypes = new[]
            {
                MediaType.ValueOf("application/json")
            };
        }

        public static IReadOnlyCollection<MediaType> SupportedMediaTypes { get; }
    }
}