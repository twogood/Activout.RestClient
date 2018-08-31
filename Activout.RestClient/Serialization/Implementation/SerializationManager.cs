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
            Serializers = new List<ISerializer> {new JsonSerializer(), new StringSerializer()};
            Deserializers = new List<IDeserializer> {new JsonDeserializer(), new StringDeserializer(), new ByteArrayDeserializer()};
        }

        public IDeserializer GetDeserializer(string mediaType)
        {
            CheckNotNull(mediaType);

            var inputMediaType = new MediaType(mediaType);

            foreach (var deserializer in Deserializers)
            {
                foreach (var supportedMediaTypeString in deserializer.SupportedMediaTypes)
                {
                    var supportedMediaType = new MediaType(supportedMediaTypeString);
                    if (inputMediaType.IsSubsetOf(supportedMediaType))
                    {
                        return deserializer;
                    }
                }
            }

            return null;
        }

        public ISerializer GetSerializer(MediaTypeCollection mediaTypeCollection)
        {
            if (mediaTypeCollection == null) return null;

            foreach (var serializer in Serializers)
            {
                if (IsMediaTypeSupported(mediaTypeCollection, serializer.SupportedMediaTypes)) return serializer;
            }

            return null;
        }

        private static bool IsMediaTypeSupported(MediaTypeCollection mediaTypeCollection,
            MediaTypeCollection supportedMediaTypes)
        {
            foreach (var supportedMediaTypeString in supportedMediaTypes)
            {
                var supportedMediaType = new MediaType(supportedMediaTypeString);
                foreach (var mediaType in mediaTypeCollection)
                {
                    var inputMediaType = new MediaType(mediaType);
                    if (inputMediaType.IsSubsetOf(supportedMediaType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}