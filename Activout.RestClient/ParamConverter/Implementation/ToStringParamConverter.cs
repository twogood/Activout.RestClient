using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class ToStringParamConverter : IParamConverter
    {
        public bool CanConvert(ParameterInfo parameterInfo)
        {
            return true;
        }

        public string ToString(object value)
        {
            return value == null ? "" : value.ToString();
        }
    }
}