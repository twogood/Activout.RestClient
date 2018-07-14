using System;
using System.Collections.Generic;
using System.Text;

namespace Activout.RestClient.Helpers
{
    public interface ITaskConverterFactory
    {
        ITaskConverter CreateTaskConverter(Type actualReturnType);
    }
}
