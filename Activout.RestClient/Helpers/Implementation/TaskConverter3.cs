using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Activout.RestClient.Helpers.Implementation;

/*
 * Convert from Task<object> to Task<T> where T is the Type
 *
 * Implemented by creating a TaskCompletionSource<T> and setting the result or exception
 */
public class TaskConverter3<T> : ITaskConverter
{
    [StackTraceHidden]
    public object ConvertReturnType(Task<object> task)
    {
        return ConvertReturnTypeImpl(task);
    }

    [StackTraceHidden]
    private static async Task<T> ConvertReturnTypeImpl(Task<object> task)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        try
        {
            taskCompletionSource.SetResult((T)await task);
        }
        catch (Exception e)
        {
            taskCompletionSource.SetException(e);
        }

        return await taskCompletionSource.Task;
    }
}