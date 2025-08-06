using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation
{
    // Synthetic ParameterInfo for backward compatibility when only Type is available
    public class SyntheticParameterInfo : ParameterInfo
    {
        private readonly Type _parameterType;
        
        public SyntheticParameterInfo(Type parameterType)
        {
            _parameterType = parameterType;
        }
        
        public override Type ParameterType => _parameterType;
        public override string Name => "synthetic";
        public override object[] GetCustomAttributes(bool inherit) => [];
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
        public override bool IsDefined(Type attributeType, bool inherit) => false;
    }

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
                if (paramConverter.CanConvert(parameterInfo.ParameterType, parameterInfo))
                {
                    return paramConverter;
                }
            }

            return null;
        }

        public IParamConverter GetConverter(Type type)
        {
            // Create a synthetic ParameterInfo for type-only queries
            // This provides backward compatibility while ensuring ParameterInfo is always provided
            var syntheticParameter = new SyntheticParameterInfo(type);
            
            foreach (var paramConverter in ParamConverters)
            {
                if (paramConverter.CanConvert(type, syntheticParameter))
                {
                    return paramConverter;
                }
            }

            return null;
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