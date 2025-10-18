using System;
using System.Collections.Generic;
using System.Globalization;
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

        public IParamConverter GetConverter(Type type, ParameterInfo parameterInfo)
        {
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(type, parameterInfo))
                {
                    return paramConverter;
                }
            }

            return null;
        }
    }
}