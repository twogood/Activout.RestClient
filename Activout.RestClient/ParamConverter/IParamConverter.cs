using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverter
    {
        bool CanConvert(Type type, ParameterInfo parameterInfo = null);
        string ToString(object value);
    }
}