using System;
using Xunit;

namespace Activout.RestClient.Test
{
    public class MediaTypeTests
    {
        [Fact]
        public void TestInvalidMediaType()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new MediaType("foo"));
        }

        [Fact]
        public void TestValueAndToString()
        {
            // Act
            var mediaType = new MediaType("foo/bar");

            // Assert
            Assert.Equal("foo/bar", mediaType.Value);
            Assert.Equal("foo/bar", mediaType.ToString());
        }


        [Fact]
        public void TestEqualsAndHashCode()
        {
            // Act
            var mediaType1 = new MediaType("foo/bar");
            var mediaType2 = new MediaType("foo/bar");

            // Assert
            Assert.True(mediaType1.Equals(mediaType2));
            Assert.Equal(mediaType1.GetHashCode(), mediaType2.GetHashCode());
        }
    }
}