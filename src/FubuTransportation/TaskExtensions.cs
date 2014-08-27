using System;
using System.Threading.Tasks;

namespace FubuTransportation
{
    public static class TaskExtensions
    {
        public static Task<T> ToCompletionTask<T>(this T value)
        {
            var completion = new TaskCompletionSource<T>();
            completion.SetResult(value);

            return completion.Task;
        }

        public static Task ToFaultedTask(this Exception ex)
        {
            var completion = new TaskCompletionSource<object>();
            completion.SetException(ex);

            return completion.Task;
        }
    }
}