using System;
using System.Reflection;

namespace Activout.RestClient.ParamConverter.Implementation;

internal static class Extensions
{
    // https://stackoverflow.com/questions/2883576/how-do-you-convert-epoch-time-in-c
    public static long ToUnixTime(this DateTime date)
    {
        return (date.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
    }
}

public class DateTimeEpochParamConverter : IParamConverter
{
    public bool CanConvert(ParameterInfo parameterInfo)
    {
        return parameterInfo.ParameterType == typeof(DateTime);
    }

    public string ToString(object value)
    {
        return value == null ? "" : ((DateTime)value).ToUnixTime().ToString();
    }
}