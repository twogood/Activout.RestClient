using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class DateTimeIso8601ParamConverter : IParamConverter
    {
        public bool CanConvert(Type type, ParameterInfo parameterInfo)
        {
            return type == typeof(DateTime);
        }

        public string ToString(object? value)
        {
            return value == null ? "" : ((DateTime)value).ToString("o");
        }
    }
}