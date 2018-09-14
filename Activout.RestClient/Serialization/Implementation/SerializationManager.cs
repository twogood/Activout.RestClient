using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using static Activout.RestClient.Helpers.Preconditions;

namespace Activout.RestClient.Serialization.Implementation
{
    public class SerializationManager : ISerializationManager
    {
        public static readonly IReadOnlyCollection<ISerializer> DefaultSerializers =
            new List<ISerializer> {new JsonSerializer(null), new StringSerializer()}
                .ToImmutableList();

        public static readonly IReadOnlyCollection<IDeserializer> DefaultDeserializers =
            new List<IDeserializer> {new JsonDeserializer(null), new StringDeserializer(), new ByteArrayDeserializer()}
                .ToImmutableList();

        private IReadOnlyCollection<ISerializer> Serializers { get; }
        private IReadOnlyCollection<IDeserializer> Deserializers { get; }

        public SerializationManager(IReadOnlyCollection<ISerializer> serializers = null,
            IReadOnlyCollection<IDeserializer> deserializers = null)
        {
            Serializers = serializers ?? DefaultSerializers;
            Deserializers = deserializers ?? DefaultDeserializers;
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
            if (mediaTypeCollection == null) throw new ArgumentNullException(nameof(mediaTypeCollection));

            return Serializers.FirstOrDefault(serializer =>
                IsMediaTypeSupported(mediaTypeCollection, serializer.SupportedMediaTypes));
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