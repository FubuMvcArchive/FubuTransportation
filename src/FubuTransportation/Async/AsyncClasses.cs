using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Async
{
    /* TODO
     * DONE 1.) HandlerCall.IsAsync()
     * DONE 2.) HandlerCall builds AsyncHandlerInvoker appropriately
     * DONE 3.) HandlerCall builds CascadingAsynchHandlerInvoker appropriately
     * 4.) Register IAsyncHandling
     * 5.) AsyncHandlingNode & AsynchHandlingConvention
     * DONE 6.) HandlerChain.IsAsync() : bool
     * 7.) ChainExecutionEnvelopeHandler needs to return the AsyncChainExecutionContinuation
     * 8.) some end to end tests!
     */

    public class AsyncChainExecutionContinuation : IContinuation
    {
        private readonly Func<IContinuation> _inner;

        public AsyncChainExecutionContinuation(Func<IContinuation> inner)
        {
            _inner = inner;
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            Task.Factory.StartNew(() => {
                var continuation = _inner();
                continuation.Execute(envelope, context);
            });
        }
    }



    public interface IAsyncHandling
    {
        void Push(Task task);
        void Push<T>(Task<T> task);

        void WaitForAll(); // can throw aggregate exception
    }

    // TODO -- need to register this
    public class AsyncHandling : IAsyncHandling, IDisposable
    {
        private readonly IInvocationContext _context;
        private readonly IList<Task> _tasks = new List<Task>();

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
            task.ContinueWith(x => _context.EnqueueCascading(x.Result), TaskContinuationOptions.OnlyOnRanToCompletion);

            _tasks.Add(task);
        }

        public void WaitForAll()
        {
            Task.WaitAll(_tasks.ToArray(), 5.Minutes());
        }

        // TODO -- need to watch this one.
        public void Dispose()
        {
            _tasks.Each(x => x.SafeDispose());
        }
    }
}