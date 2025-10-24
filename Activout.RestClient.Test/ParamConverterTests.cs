#nullable disable
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
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithDateTimeParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act & Assert
            Assert.True(converter.CanConvert(typeof(DateTime), parameterInfo));
        }

        [Fact]
        public void DateTimeIso8601ParamConverter_CanConvert_RejectsNonDateTimeType()
        {
            // Arrange
            var converter = new DateTimeIso8601ParamConverter();
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithStringParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act & Assert
            Assert.False(converter.CanConvert(typeof(string), parameterInfo));
        }

        [Fact]
        public void ToStringParamConverter_CanConvert_AcceptsAnyType()
        {
            // Arrange
            var converter = new ToStringParamConverter();
            var stringMethodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithStringParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var stringParameterInfo = stringMethodInfo.GetParameters()[0];
            var intMethodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithIntParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var intParameterInfo = intMethodInfo.GetParameters()[0];
            var dateTimeMethodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithDateTimeParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var dateTimeParameterInfo = dateTimeMethodInfo.GetParameters()[0];
            
            // Act & Assert
            Assert.True(converter.CanConvert(typeof(string), stringParameterInfo));
            Assert.True(converter.CanConvert(typeof(int), intParameterInfo));
            Assert.True(converter.CanConvert(typeof(DateTime), dateTimeParameterInfo));
        }

        [Fact]
        public void ParamConverterManager_GetConverter_WithTypeAndParameterInfo_UsesCorrectConverter()
        {
            // Arrange
            var manager = new ParamConverterManager();
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithDateTimeParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act
            var converter = manager.GetConverter(typeof(DateTime), parameterInfo);
            
            // Assert
            Assert.NotNull(converter);
            Assert.IsType<DateTimeIso8601ParamConverter>(converter);
        }

        [Fact]
        public void ParamConverterManager_GetConverter_WithTypeOnly_UsesCorrectConverter()
        {
            // Arrange
            var manager = new ParamConverterManager();
            var methodInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithDateTimeParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterInfo = methodInfo.GetParameters()[0];
            
            // Act
            var converter = manager.GetConverter(typeof(DateTime), parameterInfo);
            
            // Assert
            Assert.NotNull(converter);
            Assert.IsType<DateTimeIso8601ParamConverter>(converter);
        }

        [Fact]
        public void CustomParamConverter_CanAccessParameterInfo()
        {
            // Arrange
            var converter = new TestCustomParamConverter();
            var methodWithAttributeInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithCustomAttribute), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterWithAttribute = methodWithAttributeInfo.GetParameters()[0];
            var methodWithoutAttributeInfo = typeof(ParamConverterTests).GetMethod(nameof(TestMethodWithStringParam), BindingFlags.NonPublic | BindingFlags.Instance);
            var parameterWithoutAttribute = methodWithoutAttributeInfo.GetParameters()[0];
            
            // Act
            var canConvertWithAttribute = converter.CanConvert(typeof(string), parameterWithAttribute);
            var canConvertWithoutAttribute = converter.CanConvert(typeof(string), parameterWithoutAttribute);
            
            // Assert
            Assert.True(canConvertWithAttribute); // Should return true because parameter has TestAttribute
            Assert.False(canConvertWithoutAttribute); // Should return false because parameter doesn't have TestAttribute
        }

        // Test method used for reflection
        private void TestMethodWithDateTimeParam(DateTime date) { }

        // Test method used for reflection
        private void TestMethodWithStringParam(string value) { }

        // Test method used for reflection
        private void TestMethodWithIntParam(int value) { }

        // Test method used for reflection
        private void TestMethodWithCustomAttribute([TestAttribute] string value) { }

        // Custom attribute for testing
        private class TestAttribute : Attribute { }

        // Custom converter for testing
        private class TestCustomParamConverter : IParamConverter
        {
            public bool CanConvert(Type type, ParameterInfo parameterInfo)
            {
                // Only handle string types that have a TestAttribute
                if (type != typeof(string))
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