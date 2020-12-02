using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class ByteArraySerializer : ISerializer
    {
        public int Order { get; set; }

        public HttpContent Serialize(object data, Encoding encoding, MediaType mediaType)
        {
            var bytes = data switch
            {
                null => new byte[0],
                byte[] b => b,
                _ => encoding.GetBytes(data.ToString() ?? string.Empty)
            };

            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType.Value);
            return content;
        }

        public bool CanSerialize(MediaType mediaType)
        {
            return mediaType.Value == "application/octet-stream";
        }
    }
}