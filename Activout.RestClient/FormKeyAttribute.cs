using System;

namespace Activout.RestClient
{
    /// <summary>
    /// Specifies the key name for a property when serialized to form-url-encoded content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FormKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets the key name for the property.
        /// </summary>
        public string KeyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormKeyAttribute"/> class with the specified key name.
        /// </summary>
        /// <param name="keyName">The key name for the property.</param>
        public FormKeyAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }
}
