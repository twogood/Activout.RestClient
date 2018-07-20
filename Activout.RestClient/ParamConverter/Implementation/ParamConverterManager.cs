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
            ParamConverters = new List<IParamConverter> {new DateTimeISO8601ParamConverter(), new ToStringParamConverter()};
        }

        public IParamConverter GetConverter(ParameterInfo parameterInfo)
        {
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(parameterInfo))
                {
                    return paramConverter;
                }
            }

            return null;
        }
    }
}