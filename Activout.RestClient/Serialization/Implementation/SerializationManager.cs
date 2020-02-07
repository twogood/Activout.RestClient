using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    public class SerializationManager : ISerializationManager
    {
        public static readonly IReadOnlyCollection<JsonConverter> DefaultJsonConverters = new List<JsonConverter>
            {new SimpleValueObjectConverter()}.ToImmutableList();

        private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = DefaultJsonConverters.ToList()
        };

        public static readonly IReadOnlyCollection<ISerializer> DefaultSerializers = new List<ISerializer>
            {
                new FormUrlEncodedSerializer(),
                new JsonSerializer(DefaultJsonSerializerSettings),
                new StringSerializer()
            }
            .ToImmutableList();

        public static readonly IReadOnlyCollection<IDeserializer> DefaultDeserializers =
            new List<IDeserializer>
                {
                    new JsonDeserializer(DefaultJsonSerializerSettings), new StringDeserializer(),
                    new ByteArrayDeserializer()
                }
                .ToImmutableList();


        private IReadOnlyCollection<ISerializer> Serializers { get; }
        private IReadOnlyCollection<IDeserializer> Deserializers { get; }

        public SerializationManager(IReadOnlyCollection<ISerializer> serializers = null,
            IReadOnlyCollection<IDeserializer> deserializers = null)
        {
            Serializers = serializers ?? DefaultSerializers;
            Deserializers = deserializers ?? DefaultDeserializers;
        }

        public IDeserializer GetDeserializer(MediaType mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
            return Deserializers.FirstOrDefault(serializer => serializer.CanDeserialize(mediaType));
        }

        public ISerializer GetSerializer(MediaType mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));
            return Serializers.FirstOrDefault(serializer => serializer.CanSerialize(mediaType));
        }
    }
}