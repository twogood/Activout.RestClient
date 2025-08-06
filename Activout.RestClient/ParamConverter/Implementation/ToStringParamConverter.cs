using System;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class ToStringParamConverter : IParamConverter
    {
        public bool CanConvert(Type type)
        {
            return true;
        }

        public string ToString(object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}