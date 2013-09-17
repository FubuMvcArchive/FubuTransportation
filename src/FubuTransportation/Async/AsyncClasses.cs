using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Async
{
    /* TODO
     * 1.) HandlerCall.IsAsync()
     * 2.) HandlerCall builds AsyncHandlerInvoker appropriately
     * 3.) HandlerCall builds CascadingAsynchHandlerInvoker appropriately
     * 4.) Register IAsyncHandling
     * 5.) AsyncHandlingNode & AsynchHandlingConvention
     * 6.) HandlerChain.IsAsync() : bool
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

    public class AsyncHandlerInvoker<TController, TInput> : BasicBehavior where TInput : class
    {
        private readonly TController _controller;
        private readonly Func<TController, TInput, Task> _func;
        private readonly IFubuRequest _request;
        private readonly IAsyncHandling _asyncHandling;

        public AsyncHandlerInvoker(IFubuRequest request, IAsyncHandling asyncHandling, TController controller,
                                    Func<TController, TInput, Task> func)
            : base(PartialBehavior.Executes)
        {
            _request = request;
            _asyncHandling = asyncHandling;
            _controller = controller;
            _func = func;
        }

        protected override DoNext performInvoke()
        {
            var input = _request.Find<TInput>().Single();
            var task = _func(_controller, input);
            _asyncHandling.Push(task);

            return DoNext.Continue;
        }
    }

    public class CascadingAsyncHandlerInvoker<THandler, TInput, TOutput> : BasicBehavior where TInput : class
    {
        private readonly IFubuRequest _request;
        private readonly THandler _handler;
        private readonly Func<THandler, TInput, Task<TOutput>> _func;
        private readonly IAsyncHandling _asyncHandling;

        public CascadingAsyncHandlerInvoker(IFubuRequest request, THandler handler, Func<THandler, TInput, Task<TOutput>> func, IAsyncHandling asyncHandling)
            : base(PartialBehavior.Executes)
        {
            _request = request;
            _handler = handler;
            _func = func;
            _asyncHandling = asyncHandling;
        }

        protected override DoNext performInvoke()
        {
            var input = _request.Find<TInput>().Single();
            var output = _func(_handler, input);

            _asyncHandling.Push(output);

            return DoNext.Continue;
        }
    }
}