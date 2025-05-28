using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Activout.RestClient.Serialization;
using Newtonsoft.Json;

namespace Activout.RestClient.Newtonsoft.Json
{
    public class NewtonsoftSerializationManager : ISerializationManager
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
                new NewtonsoftJsonSerializer(DefaultJsonSerializerSettings)
            }
            .ToImmutableList();

        public static readonly IReadOnlyCollection<IDeserializer> DefaultDeserializers =
            new List<IDeserializer>
                {
                    new NewtonsoftJsonDeserializer(DefaultJsonSerializerSettings)
                }
                .ToImmutableList();


        private IReadOnlyCollection<ISerializer> Serializers { get; }
        private IReadOnlyCollection<IDeserializer> Deserializers { get; }

        public NewtonsoftSerializationManager(IReadOnlyCollection<ISerializer> serializers = null,
            IReadOnlyCollection<IDeserializer> deserializers = null)
        {
            // Combine with core serializers/deserializers if provided
            var allSerializers = new List<ISerializer>();
            if (serializers != null)
            {
                allSerializers.AddRange(serializers);
            }
            allSerializers.AddRange(DefaultSerializers);
            
            var allDeserializers = new List<IDeserializer>();
            if (deserializers != null)
            {
                allDeserializers.AddRange(deserializers);
            }
            allDeserializers.AddRange(DefaultDeserializers);
            
            Serializers = allSerializers.OrderBy(s => s.Order).ToArray();
            Deserializers = allDeserializers.OrderBy(s => s.Order).ToArray();
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
