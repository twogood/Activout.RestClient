using System.Collections.Generic;

namespace Activout.RestClient.Newtonsoft.Json
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
