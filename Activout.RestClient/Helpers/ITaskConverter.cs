using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Activout.RestClient.Helpers
{
    public interface ITaskConverter
    {
        object ConvertReturnType(Task<object> task);
    }
}
