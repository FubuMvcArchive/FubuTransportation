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
    }
}