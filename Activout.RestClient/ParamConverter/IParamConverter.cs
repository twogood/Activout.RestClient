using System.Reflection;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverter
    {
        bool CanConvert(ParameterInfo parameterInfo);
        string ToString(object value);
    }
}