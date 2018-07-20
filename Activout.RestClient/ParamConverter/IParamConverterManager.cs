﻿using System.Reflection;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverterManager
    {
        IParamConverter GetConverter(ParameterInfo parameterInfo);
    }
}