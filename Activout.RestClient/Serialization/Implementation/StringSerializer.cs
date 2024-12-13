using System.Net.Http;
using System.Text;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class StringSerializer : ISerializer
    {
        public int Order { get; set; } = 1000;

        public HttpContent Serialize(object data, Encoding encoding, MediaType mediaType)
        {
            return new StringContent(data.ToString(), encoding, mediaType.Value);
        }

        public bool CanSerialize(MediaType mediaType)
        {
            return mediaType.Value.StartsWith("text/");
        }
    }
}