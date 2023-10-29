using System;

namespace Activout.RestClient.Helpers.Implementation;

public class TaskConverterFactory : ITaskConverterFactory
{
    public ITaskConverter CreateTaskConverter(Type actualReturnType)
    {
        return actualReturnType == typeof(void) ? null : new TaskConverter(actualReturnType);
    }
}