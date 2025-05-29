using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Activout.RestClient.Json;

/// <summary>
/// JSON converter for simple value objects.
/// A "simple value object" is defined as an object with:
/// 1. No default constructor
/// 2. A public property named Value
/// 3. A constructor taking the same type as the Value property
/// 4. A type that is not Nullable&lt;T&gt;
/// </summary>
public class SimpleValueObjectConverter : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the converter can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns>True if the type can be converted, otherwise false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        if (Nullable.GetUnderlyingType(typeToConvert) != null) return false;
        if (GetDefaultConstructor(typeToConvert) != null) return false;

        var valueProperty = GetValueProperty(typeToConvert);
        if (valueProperty == null) return false;

        var constructor = GetValueConstructor(typeToConvert, valueProperty);
        return constructor != null;
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A converter for the specified type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueProperty = GetValueProperty(typeToConvert);
        var valueType = valueProperty!.PropertyType;

        var converterType = typeof(SimpleValueObjectConverterInner<,>).MakeGenericType(
            typeToConvert,
            valueType);

        return (JsonConverter)Activator.CreateInstance(converterType);
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
        return objectType.GetConstructor(Type.EmptyTypes);
    }

    // Inner converter that handles the actual conversion
    private class SimpleValueObjectConverterInner<TObject, TValue> : JsonConverter<TObject>
    {
        public override TObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            if (value == null)
                return default;

            return (TObject?)Activator.CreateInstance(typeof(TObject), value);
        }

        public override void Write(Utf8JsonWriter writer, TObject value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var propertyValue = typeof(TObject).GetProperty("Value").GetValue(value);
            JsonSerializer.Serialize(writer, propertyValue, options);
        }
    }
}