using System;

namespace Activout.RestClient.Helpers.Implementation
{
    internal class TaskConverterFactory : ITaskConverterFactory
    {
        public ITaskConverter CreateTaskConverter(Type actualReturnType)
        {
            if (actualReturnType == typeof(void)) return null;

            return new TaskConverter(actualReturnType);
        }
    }
}