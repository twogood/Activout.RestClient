using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    /*
     * We define a "simple value object" as an object that has:
     * 
     * 1. No default constructor 
     * 2. A public property named Value
     * 3. A constructor taking the same type as the Value property
     * 4. A type that is not Nullable<T>
     */
    public class SimpleValueObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var valueProperty = GetValueProperty(value.GetType());
            serializer.Serialize(writer, valueProperty.GetValue(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            var valueProperty = GetValueProperty(objectType);
            var value = serializer.Deserialize(reader, valueProperty.PropertyType);
            return value == null ? null : Activator.CreateInstance(objectType, value);
        }

        public override bool CanConvert(Type objectType)
        {
            if (Nullable.GetUnderlyingType(objectType) != null) return false;
            if (GetDefaultConstructor(objectType) != null) return false;
            var valueProperty = GetValueProperty(objectType);
            if (valueProperty == null) return false;
            var constructor = GetValueConstructor(objectType, valueProperty);
            return constructor != null;
        }

        private static ConstructorInfo GetValueConstructor(Type objectType, PropertyInfo valueProperty)
        {
            return objectType.GetConstructor(new[] {valueProperty.PropertyType});
        }

        private static PropertyInfo GetValueProperty(Type objectType)
        {
            return objectType.GetProperty("Value");
        }

        private static ConstructorInfo GetDefaultConstructor(Type objectType)
        {
            return objectType.GetConstructor(new Type[0]);
        }
    }
}