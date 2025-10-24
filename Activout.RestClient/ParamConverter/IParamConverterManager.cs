using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverterManager
    {
        IParamConverter? GetConverter(Type type, ParameterInfo parameterInfo);
    }
}