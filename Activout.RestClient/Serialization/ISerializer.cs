using System.Net.Http;
using System.Text;

namespace Activout.RestClient.Serialization
{
    public interface ISerializer
    {
        HttpContent Serialize(object data, Encoding encoding, MediaType mediaType);
        bool CanSerialize(MediaType mediaType);
    }
}