using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Activout.RestClient.Helpers.Implementation
{
    /*
     * Convert from Task<object> to Task<T> where T is the Type
     * 
     * Implemented by creating a TaskCompletionSource<T> and calling SetResult() using reflection.
     */
    public class TaskConverter2 : ITaskConverter
    {
        private readonly Type _type;
        private readonly MethodInfo _setResultMethod;
        private readonly MethodInfo _setExceptionMethod;
        private readonly PropertyInfo _taskProperty;

        public TaskConverter2(Type actualReturnType)
        {
            _type = typeof(TaskCompletionSource<>).MakeGenericType(actualReturnType);
            _setResultMethod = _type.GetMethod("SetResult") ??
                               throw new InvalidOperationException("Method SetResult not found");
            _setExceptionMethod = _type.GetMethod("SetException", new[] {typeof(Exception)}) ??
                                  throw new InvalidOperationException("Method SetException not found");
            _taskProperty = _type.GetProperty("Task") ?? throw new InvalidOperationException("Property Task not found");
        }

        public object ConvertReturnType(Task<object> task)
        {
            var taskCompletionSource = Activator.CreateInstance(_type);

            Task.Factory.StartNew(async () => await AsyncHelper(task, taskCompletionSource));

            return GetTask(taskCompletionSource);
        }

        private async Task AsyncHelper(Task<object> task, object taskCompletionSource)
        {
            try
            {
                SetResult(taskCompletionSource, await task);
            }
            catch (Exception e)
            {
                SetException(taskCompletionSource, e);
            }
        }

        private object GetTask(object taskCompletionSource)
        {
            return _taskProperty.GetValue(taskCompletionSource);
        }

        private void SetException(object taskCompletionSource, Exception e)
        {
            _setExceptionMethod.Invoke(taskCompletionSource, new object[] {e});
        }

        private void SetResult(object taskCompletionSource, object result)
        {
            _setResultMethod.Invoke(taskCompletionSource, new[] {result});
        }
    }
}