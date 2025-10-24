using System.Text;
using Activout.RestClient.Serialization;
using Newtonsoft.Json;

namespace Activout.RestClient.Newtonsoft.Json
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public IReadOnlyCollection<MediaType> SupportedMediaTypes => NewtonsoftJsonDefaults.SupportedMediaTypes;

        public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public int Order { get; set; }

        public HttpContent Serialize(object? data, Encoding encoding, MediaType mediaType)
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
