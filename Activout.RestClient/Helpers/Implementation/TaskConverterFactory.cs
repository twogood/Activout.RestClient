using System;

namespace Activout.RestClient.Helpers.Implementation
{
    class TaskConverterFactory : ITaskConverterFactory
    {
        public ITaskConverter CreateTaskConverter(Type actualReturnType)
        {
            if (actualReturnType == typeof(void))
            {
                // TODO: return a dummy converter?
                return null;
            }

            return new TaskConverter(actualReturnType);
        }
    }
}
