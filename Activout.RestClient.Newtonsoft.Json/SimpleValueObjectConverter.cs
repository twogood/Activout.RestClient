using System.Reflection;
using Newtonsoft.Json;

namespace Activout.RestClient.Newtonsoft.Json
{
    /*
     * We define a "simple value object" as an object that has:
     * 
     * 1. No default constructor 
     * 2. A public property named Value
     * 3. A constructor taking the same type as the Value property
     *
     * Nullable<T> wrappers of such types are also supported.
     */
    public class SimpleValueObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var valueProperty = GetValueProperty(value.GetType());
            serializer.Serialize(writer, valueProperty!.GetValue(value));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;
            var valueProperty = GetValueProperty(objectType);
            var value = serializer.Deserialize(reader, valueProperty!.PropertyType);
            return value == null ? null : Activator.CreateInstance(objectType, value);
        }

        public override bool CanConvert(Type objectType)
        {
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;
            if (GetDefaultConstructor(objectType) != null) return false;
            var valueProperty = GetValueProperty(objectType);
            if (valueProperty == null) return false;
            var constructor = GetValueConstructor(objectType, valueProperty);
            return constructor != null;
        }

        private static ConstructorInfo? GetValueConstructor(Type objectType, PropertyInfo valueProperty)
        {
            return objectType.GetConstructor([valueProperty.PropertyType]);
        }

        private static PropertyInfo? GetValueProperty(Type objectType)
        {
            return objectType.GetProperty("Value");
        }

        private static ConstructorInfo? GetDefaultConstructor(Type objectType)
        {
            return objectType.GetConstructor([]);
        }
    }
}
