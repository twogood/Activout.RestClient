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
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(parameterInfo))
                {
                    return paramConverter;
                }
            }

            return null;
        }

        public IParamConverter GetConverter(Type type)
        {
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(CreateParameterInfoForType(type)))
                {
                    return paramConverter;
                }
            }

            return null;
        }

        private static ParameterInfo CreateParameterInfoForType(Type type)
        {
            // Create a minimal ParameterInfo-like object that only provides the Type
            // This is used internally to reuse existing CanConvert logic
            return new TypeOnlyParameterInfo(type);
        }

        private class TypeOnlyParameterInfo : ParameterInfo
        {
            private readonly Type _parameterType;

            public TypeOnlyParameterInfo(Type parameterType)
            {
                _parameterType = parameterType;
            }

            public override Type ParameterType => _parameterType;
        }
    }
}