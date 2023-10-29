using System;

namespace Activout.RestClient.Helpers.Implementation;

public class TaskConverter2Factory : ITaskConverterFactory
{
    public ITaskConverter CreateTaskConverter(Type actualReturnType)
    {
        return actualReturnType == typeof(void) ? null : new TaskConverter2(actualReturnType);
    }
}