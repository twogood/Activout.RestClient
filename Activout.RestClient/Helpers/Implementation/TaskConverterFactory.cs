#nullable disable
using System;

namespace Activout.RestClient.Helpers.Implementation
{
    [Obsolete("This class is obsolete and will be removed in a future version. Use TaskConverter3Factory instead.")]
    public class TaskConverterFactory : ITaskConverterFactory
    {
        public ITaskConverter CreateTaskConverter(Type actualReturnType)
        {
            return actualReturnType == typeof(void) ? null : new TaskConverter(actualReturnType);
        }
    }
}