using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Activout.RestClient.Test
{
    // https://blogs.msdn.microsoft.com/agileer/2013/05/17/the-correct-way-to-code-a-custom-exception-class/
    public class RestClientExceptionTest
    {
        [Fact]
        public void RestClientException_default_ctor()
        {
            // Arrange
            const string expectedMessage = "Exception of type 'Activout.RestClient.RestClientException' was thrown.";

            // Act
            var sut = new RestClientException(HttpStatusCode.InternalServerError, null);

            // Assert
            Assert.Null(sut.ErrorResponse);
            Assert.Null(sut.InnerException);
            Assert.Equal(HttpStatusCode.InternalServerError, sut.StatusCode);
            Assert.Equal(expectedMessage, sut.Message);
        }

        [Fact]
        public void RestClientException_ctor_string()
        {
            // Arrange
            const string expectedMessage = "message";

            // Act
            var sut = new RestClientException(HttpStatusCode.InternalServerError, expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, sut.ErrorResponse);
            Assert.Null(sut.InnerException);
            Assert.Equal(HttpStatusCode.InternalServerError, sut.StatusCode);
            Assert.Equal(expectedMessage, sut.Message);
        }

        [Fact]
        public void RestClientException_ctor_string_ex()
        {
            // Arrange
            const string expectedMessage = "message";
            var innerEx = new Exception("foo");

            // Act
            var sut = new RestClientException(HttpStatusCode.InternalServerError, expectedMessage, innerEx);

            // Assert
            Assert.Equal(expectedMessage, sut.ErrorResponse);
            Assert.Equal(HttpStatusCode.InternalServerError, sut.StatusCode);
            Assert.Equal(innerEx, sut.InnerException);
            Assert.Equal(expectedMessage, sut.Message);
        }

        [Fact]
        public void RestClientException_serialization_deserialization_test()
        {
            // Arrange
            var innerEx = new Exception("foo");
            var originalException =
                new RestClientException(HttpStatusCode.InternalServerError, "message", innerEx);
            var buffer = new byte[4096];
            var ms = new MemoryStream(buffer);
            var ms2 = new MemoryStream(buffer);
            var formatter = new BinaryFormatter();

            // Act
            formatter.Serialize(ms, originalException);
            var deserializedException = (RestClientException) formatter.Deserialize(ms2);

            // Assert
            Assert.Equal(originalException.StatusCode, deserializedException.StatusCode);
            Assert.Equal(originalException.InnerException.Message, deserializedException.InnerException.Message);
            Assert.Equal(originalException.Message, deserializedException.Message);
            Assert.Equal(originalException.ErrorResponse, deserializedException.ErrorResponse);
        }
    }
}