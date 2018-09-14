using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public MediaTypeCollection SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public JsonSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public HttpContent Serialize(object data, Encoding encoding, string mediaType)
        {
            return new StringContent(
                JsonConvert.SerializeObject(data, _jsonSerializerSettings),
                encoding, mediaType);
        }
    }
}