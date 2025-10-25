using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Activout.RestClient.Serialization;

public interface IDeserializer
{
    int Order { get; }
    Task<object?> Deserialize(HttpContent content, Type type);
    bool CanDeserialize(MediaType mediaType);
}