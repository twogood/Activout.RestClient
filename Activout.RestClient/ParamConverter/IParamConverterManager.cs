using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverterManager
    {
        IParamConverter GetConverter(ParameterInfo parameterInfo);
        IParamConverter GetConverter(Type type);
    }
}