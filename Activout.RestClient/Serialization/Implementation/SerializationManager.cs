using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class SerializationManager : ISerializationManager
    {
        public List<IDeserializer> Deserializers { get; }
        public List<ISerializer> Serializers { get; }

        public SerializationManager()
        {
            Serializers = new List<ISerializer> {new JsonSerializer(), new TextSerializer()};
            Deserializers = new List<IDeserializer> {new JsonDeserializer()};
        }

        public IDeserializer GetDeserializer(string mediaType)
        {
            CheckNotNull(mediaType);
            return Deserializers.First(s => s.SupportedMediaTypes.Contains(mediaType));
        }

        public ISerializer GetSerializer(MediaTypeCollection mediaTypeCollection)
        {
            if (mediaTypeCollection == null) return Serializers.First();

            foreach (var serializer in Serializers)
            {
                foreach (var supportedMediaTypeString in serializer.SupportedMediaTypes)
                {
                    var supportedMediaType = new MediaType(supportedMediaTypeString);
                    foreach (var mediaType in mediaTypeCollection)
                    {
                        var inputMediaType = new MediaType(mediaType);
                        if (inputMediaType.IsSubsetOf(supportedMediaType)) return serializer;
                    }
                }
            }

            return null;
        }
    }
}