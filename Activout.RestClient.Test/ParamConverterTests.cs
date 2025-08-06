using System;
using System.Reflection;
using Activout.RestClient.ParamConverter;
using Activout.RestClient.ParamConverter.Implementation;
using Xunit;

namespace Activout.RestClient.Test
{
    public class ParamConverterTests
    {
        [Fact]
        public void DateTimeIso8601ParamConverter_CanConvert_AcceptsDateTimeType()
        {
            // Arrange
            var converter = new DateTimeIso8601ParamConverter();
            
            // Act & Assert
            Assert.True(converter.CanConvert(typeof(DateTime)));
            Assert.True(converter.CanConvert(typeof(DateTime), null));
        }

        [Fact]
        public void DateTimeIso8601ParamConverter_CanConvert_RejectsNonDateTimeType()
        {
            // Arrange
            var converter = new DateTimeIso8601ParamConverter();
            
            // Act & Assert
            Assert.False(converter.CanConvert(typeof(string)));
            Assert.False(converter.CanConvert(typeof(string), null));
        }

        [Fact]
        public void ToStringParamConverter_CanConvert_AcceptsAnyType()
        {
            // Arrange
            var converter = new ToStringParamConverter();
            
            // Act & Assert
            Assert.True(converter.CanConvert(typeof(string)));
            Assert.True(converter.CanConvert(typeof(int)));
            Assert.True(converter.CanConvert(typeof(DateTime)));
            Assert.True(converter.CanConvert(typeof(string), null));
        }

        [Fact]
        public void ParamConverterManager_GetConverter_WithParameterInfo_UsesCorrectConverter()
        {
            // Arrange
            var manager = new ParamConverterManager();
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithDateTimeParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act
            var converter = manager.GetConverter(parameterInfo);
            
            // Assert
            Assert.NotNull(converter);
            Assert.IsType<DateTimeIso8601ParamConverter>(converter);
        }

        [Fact]
        public void ParamConverterManager_GetConverter_WithType_UsesCorrectConverter()
        {
            // Arrange
            var manager = new ParamConverterManager();
            
            // Act
            var converter = manager.GetConverter(typeof(DateTime));
            
            // Assert
            Assert.NotNull(converter);
            Assert.IsType<DateTimeIso8601ParamConverter>(converter);
        }

        [Fact]
        public void CustomParamConverter_CanAccessParameterInfo()
        {
            // Arrange
            var converter = new TestCustomParamConverter();
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithCustomAttribute), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act
            var canConvertWithParameterInfo = converter.CanConvert(typeof(string), parameterInfo);
            var canConvertWithoutParameterInfo = converter.CanConvert(typeof(string), null);
            
            // Assert
            Assert.True(canConvertWithParameterInfo); // Should return true because parameter has TestAttribute
            Assert.False(canConvertWithoutParameterInfo); // Should return false because no parameter info
        }

        // Test method used for reflection
        private void TestMethodWithDateTimeParam(DateTime date) { }

        // Test method used for reflection
        private void TestMethodWithCustomAttribute([TestAttribute] string value) { }

        // Custom attribute for testing
        private class TestAttribute : Attribute { }

        // Custom converter for testing
        private class TestCustomParamConverter : IParamConverter
        {
            public bool CanConvert(Type type, ParameterInfo parameterInfo = null)
            {
                // Only handle string types that have a TestAttribute
                if (type != typeof(string))
                    return false;

                // If no parameter info, can't determine if it has the attribute
                if (parameterInfo == null)
                    return false;

                // Check if parameter has TestAttribute
                return parameterInfo.GetCustomAttribute<TestAttribute>() != null;
            }

            public string ToString(object value)
            {
                return value?.ToString() ?? "";
            }
        }
    }
}