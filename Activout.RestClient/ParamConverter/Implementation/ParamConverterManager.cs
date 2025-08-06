using System;
using System.Collections.Generic;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    public class ParamConverterManager : IParamConverterManager
    {
        public List<IParamConverter> ParamConverters { get; }

        public ParamConverterManager()
        {
            ParamConverters = new List<IParamConverter> { new DateTimeIso8601ParamConverter(), new ToStringParamConverter() };
        }

        public IParamConverter GetConverter(ParameterInfo parameterInfo)
        {
            return GetConverter(parameterInfo.ParameterType);
        }

        public IParamConverter GetConverter(Type type)
        {
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(type))
                {
                    return paramConverter;
                }
            }

            return null;
        }
    }
}