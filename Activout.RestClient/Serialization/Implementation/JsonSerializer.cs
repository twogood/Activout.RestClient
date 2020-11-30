using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public IReadOnlyCollection<MediaType> SupportedMediaTypes => JsonHelper.SupportedMediaTypes;

        public JsonSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public int Order { get; set; }

        public HttpContent Serialize(object data, Encoding encoding, MediaType mediaType)
        {
            return new StringContent(
                JsonConvert.SerializeObject(data, _jsonSerializerSettings),
                encoding, mediaType.Value);
        }

        public bool CanSerialize(MediaType mediaType)
        {
            return SupportedMediaTypes.Contains(mediaType);
        }
    }
}