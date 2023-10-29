using System.Threading.Tasks;

namespace Activout.RestClient.Helpers;

public interface ITaskConverter
{
    Task ConvertReturnType(Task<object> task);
}