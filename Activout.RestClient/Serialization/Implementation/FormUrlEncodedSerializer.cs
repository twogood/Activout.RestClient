using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Activout.RestClient.Serialization.Implementation
{
    public class FormUrlEncodedSerializer : ISerializer
    {
        public IReadOnlyCollection<MediaType> SupportedMediaTypes => new[]
        {
            MediaType.ValueOf("application/x-www-form-urlencoded")
        };

        public HttpContent Serialize(object data, Encoding encoding, MediaType mediaType)
        {
            if (data == null)
            {
                return new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>());
            }

            if (data is IEnumerable<KeyValuePair<string, string>> enumerable)
            {
                return new FormUrlEncodedContent(enumerable);
            }

            var type = data.GetType();
            var properties = type.GetProperties();

            return new FormUrlEncodedContent(
                properties
                    .Select(p => new {Property = p, Value = p.GetValue(data)})
                    .Where(x => x.Value != null)
                    .Select(x => new KeyValuePair<string, string>(GetKey(x.Property), SerializeValue(x.Value)))
            );
        }

        public bool CanSerialize(MediaType mediaType)
        {
            return SupportedMediaTypes.Contains(mediaType);
        }

        private static string SerializeValue(object value)
        {
            return value.ToString();
        }

        private static string GetKey(MemberInfo property)
        {
            var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
            return attribute == null ? property.Name : attribute.PropertyName;
        }
    }
}