using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Activout.RestClient.Implementation;

/*
 * Convert from Task<object?> to Task<T?> where T is the actual return type
 */
internal static class TaskConverter
{
    private static readonly MethodInfo ConvertMethod =
        typeof(TaskConverter).GetMethod(nameof(Convert), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static Func<Task<object?>, object> Create(Type actualReturnType) =>
        (Func<Task<object?>, object>)ConvertMethod.MakeGenericMethod(actualReturnType)
            .CreateDelegate(typeof(Func<Task<object?>, object>));

    // Two methods: the async one cannot return object.
    [StackTraceHidden]
    private static object Convert<T>(Task<object?> task) => ConvertAsync<T>(task);

    [StackTraceHidden]
    private static async Task<T?> ConvertAsync<T>(Task<object?> task) => (T?)await task;
}
