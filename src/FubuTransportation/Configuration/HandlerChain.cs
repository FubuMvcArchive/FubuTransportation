using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Configuration
{
    public class HandlerChain : BehaviorChain, IMayHaveInputType
    {
        public static readonly string Category = "Handler";
        public readonly IList<IErrorHandler> ErrorHandlers = new List<IErrorHandler>();
        public int MaximumAttempts = 1;

        public HandlerChain()
        {
            UrlCategory.Category = Category;
        }

        public HandlerChain(IEnumerable<HandlerCall> calls) : this()
        {
            calls.Each(AddToEnd);
        }

        public ContinuationExpression OnException<T>() where T : Exception
        {
            return new OnExceptionExpression<T>(this);
        }

        public interface ThenContinueExpression
        {
            ContinuationExpression Then { get; }
        }

        public interface ContinuationExpression
        {
            ThenContinueExpression Retry();
            ThenContinueExpression Requeue();
            ThenContinueExpression MoveToErrorQueue();
            ThenContinueExpression RetryLater(TimeSpan delay);
            ThenContinueExpression ContinueWith(IContinuation continuation);
            ThenContinueExpression ContinueWith<TContinuation>() where TContinuation : IContinuation, new();
        }

        public class OnExceptionExpression<T> : ContinuationExpression, ThenContinueExpression where T : Exception
        {
            private readonly HandlerChain _parent;
            private readonly Lazy<ErrorHandler> _handler; 

            public OnExceptionExpression(HandlerChain parent)
            {
                _parent = parent;

                _handler = new Lazy<ErrorHandler>(() => {
                    var handler = new ErrorHandler();
                    handler.AddCondition(new ExceptionTypeMatch<T>());
                    _parent.ErrorHandlers.Add(handler);

                    return handler;
                });
            }

            public ThenContinueExpression Retry()
            {
                return ContinueWith(new RetryNowContinuation());
            }

            public ThenContinueExpression Requeue()
            {
                return ContinueWith(new RequeueContinuation());
            }

            public ThenContinueExpression MoveToErrorQueue()
            {
                _parent.ErrorHandlers.Add(new MoveToErrorQueueHandler<T>());

                return this;
            }

            public ThenContinueExpression RetryLater(TimeSpan delay)
            {
                return ContinueWith(new DelayedRetryContinuation(delay));
            }

            public ThenContinueExpression ContinueWith(IContinuation continuation)
            {
                _handler.Value.AddContinuation(continuation);

                return this;
            }

            public ThenContinueExpression ContinueWith<TContinuation>() where TContinuation : IContinuation, new()
            {
                return ContinueWith(new TContinuation());
            }

            ContinuationExpression ThenContinueExpression.Then
            {
                get
                {
                    return this;
                }
            }
        }

        public bool IsAsync
        {
            get
            {
                return this.OfType<HandlerCall>().Any(x => x.IsAsync);
            }
        }
    }
}