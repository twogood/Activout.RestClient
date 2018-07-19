using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Serialization.Implementation
{
    internal class SerializationManager : ISerializationManager
    {
        private readonly List<IDeserializer> _deserializers;
        private readonly List<ISerializer> _serializers;

        public SerializationManager(List<ISerializer> serializers = null, List<IDeserializer> deserializers = null)
        {
            this._serializers = serializers ?? new List<ISerializer>();
            this._deserializers = deserializers ?? new List<IDeserializer>();

            this._serializers.Add(new JsonSerializer());
            this._serializers.Add(new TextSerializer());

            this._deserializers.Add(new JsonDeserializer());
        }

        public IDeserializer GetDeserializer(string mediaType)
        {
            CheckNotNull(mediaType);
            return _deserializers.First(s => s.SupportedMediaTypes.Contains(mediaType));
        }

        public ISerializer GetSerializer(MediaTypeCollection mediaTypeCollection)
        {
            if (mediaTypeCollection == null) return _serializers.First();

            foreach (var serializer in _serializers)
            foreach (var supportedMediaTypeString in serializer.SupportedMediaTypes)
            {
                var supportedMediaType = new MediaType(supportedMediaTypeString);
                foreach (var mediaType in mediaTypeCollection)
                {
                    var inputMediaType = new MediaType(mediaType);
                    if (inputMediaType.IsSubsetOf(supportedMediaType)) return serializer;
                }
            }

            return null;
        }
    }
}