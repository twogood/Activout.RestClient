using System.Collections.Generic;

namespace Activout.RestClient.Serialization
{
    public interface ISerializationManager
    {
        ISerializer GetSerializer(MediaType mediaType);
        IDeserializer GetDeserializer(MediaType mediaType);
    }
}