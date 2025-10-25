using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Activout.RestClient.Serialization.Implementation
{
    public class SerializationManager : ISerializationManager
    {
        public static readonly IReadOnlyCollection<ISerializer> DefaultSerializers = new List<ISerializer>
            {
                new FormUrlEncodedSerializer(),
                StringSerializer.Instance,
                new ByteArraySerializer()
            }
            .ToImmutableList();

        public static readonly IReadOnlyCollection<IDeserializer> DefaultDeserializers =
            new List<IDeserializer>
                {
                    new StringDeserializer(),
                    new ByteArrayDeserializer()
                }
                .ToImmutableList();


        private IReadOnlyCollection<ISerializer> Serializers { get; }
        private IReadOnlyCollection<IDeserializer> Deserializers { get; }

        public SerializationManager(IReadOnlyCollection<ISerializer>? serializers = null,
            IReadOnlyCollection<IDeserializer>? deserializers = null)
        {
            Serializers = (serializers ?? DefaultSerializers).OrderBy(s => s.Order).ToArray();
            Deserializers = (deserializers ?? DefaultDeserializers).OrderBy(s => s.Order).ToArray();
        }

        public IDeserializer? GetDeserializer(MediaType mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
            return Deserializers.FirstOrDefault(serializer => serializer.CanDeserialize(mediaType));
        }

        public ISerializer? GetSerializer(MediaType mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
            return Serializers.FirstOrDefault(serializer => serializer.CanSerialize(mediaType));
        }
    }
}