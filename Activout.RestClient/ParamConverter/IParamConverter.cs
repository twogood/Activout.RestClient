using System;

namespace Activout.RestClient.ParamConverter
{
    public interface IParamConverter
    {
        bool CanConvert(Type type);
        string ToString(object value);
    }
}