using System.Collections.Generic;
using System.Linq;
using static Activout.RestClient.Helpers.Preconditions;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Activout.RestClient.Serialization.Implementation
{
    class SerializationManager : ISerializationManager
    {
        private readonly List<ISerializer> serializers;
        private readonly List<IDeserializer> deserializers;

        public SerializationManager(List<ISerializer> serializers = null, List<IDeserializer> deserializers = null)
        {
            this.serializers = serializers ?? new List<ISerializer>();
            this.deserializers = deserializers ?? new List<IDeserializer>();

            this.serializers.Add(new JsonSerializer());
            this.serializers.Add(new TextSerializer());

            this.deserializers.Add(new JsonDeserializer());
        }

        public IDeserializer GetDeserializer(string mediaType)
        {
            CheckNotNull(mediaType);
            return deserializers.First(s => s.SupportedMediaTypes.Contains(mediaType));
        }

        public ISerializer GetSerializer(MediaTypeCollection mediaTypeCollection)
        {
            if (mediaTypeCollection == null)
            {
                return serializers.First();
            }

            foreach (var serializer in serializers)
            {
                foreach (var supportedMediaTypeString in serializer.SupportedMediaTypes)
                {
                    var supportedMediaType = new MediaType(supportedMediaTypeString);
                    foreach (var mediaType in mediaTypeCollection)
                    {
                        var inputMediaType = new MediaType(mediaType);
                        if (inputMediaType.IsSubsetOf(supportedMediaType))
                        {
                            return serializer;
                        }
                    }
                }
            }
            return null;
        }
    }
}
