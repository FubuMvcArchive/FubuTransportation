using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Async
{
    public class AsyncHandling : IAsyncHandling, IDisposable
    {
        private readonly IInvocationContext _context;
        private readonly IList<Task> _tasks = new List<Task>();
        private readonly IList<Task> _messages = new List<Task>();

        public AsyncHandling(IInvocationContext context)
        {
            _context = context;
        }

        public void Push(Task task)
        {
            _tasks.Add(task);
        }

        public void Push<T>(Task<T> task)
        {
            var messages = task.ContinueWith(x => _context.EnqueueCascading(x.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            _messages.Add(messages);

            _tasks.Add(task);
        }

        public void WaitForAll()
        {
            Task.WaitAll(_tasks.ToArray(), 5.Minutes());
            Task.WaitAll(_messages.ToArray(), 1.Minutes());
        }

        // TODO -- need to watch this one.
        public void Dispose()
        {
            _tasks.Each(x => x.SafeDispose());
        }
    }
}