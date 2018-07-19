using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization
{
    public interface ISerializer
    {
        MediaTypeCollection SupportedMediaTypes { get; }
        HttpContent Serialize(object data, Encoding encoding, string mediaType);
    }
}