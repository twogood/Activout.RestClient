using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization
{
    public interface ISerializationManager
    {
        ISerializer GetSerializer(MediaTypeCollection mediaTypeCollection);
        IDeserializer GetDeserializer(string mediaType);
    }
}