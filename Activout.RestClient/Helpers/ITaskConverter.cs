using System.Threading.Tasks;

namespace Activout.RestClient.Helpers;

public interface ITaskConverter
{
    object ConvertReturnType(Task<object?> task);
}