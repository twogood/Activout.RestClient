using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class ToStringParamConverter : IParamConverter
    {
        public bool CanConvert(Type type, ParameterInfo parameterInfo = null)
        {
            return true;
        }

        public string ToString(object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}