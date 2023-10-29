using System;

namespace Activout.RestClient.Helpers;

public interface ITaskConverterFactory
{
    ITaskConverter CreateTaskConverter(Type actualReturnType);
}