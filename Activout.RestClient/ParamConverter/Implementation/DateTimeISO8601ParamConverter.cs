using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class DateTimeISO8601ParamConverter : IParamConverter
    {
        public bool CanConvert(ParameterInfo parameterInfo)
        {
            return parameterInfo.ParameterType == typeof(DateTime);
        }

        public string ToString(object value)
        {
            return value == null ? "" : ((DateTime) value).ToString("o");
        }
    }
}