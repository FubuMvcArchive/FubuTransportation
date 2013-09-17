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
            IsPartialOnly = true;
        }

        public HandlerChain(IEnumerable<HandlerCall> calls) : this()
        {
            calls.Each(AddToEnd);
        }

        public OnExceptionExpression<T> OnException<T>() where T : Exception
        {
            return new OnExceptionExpression<T>(this);
        } 

        public class OnExceptionExpression<T> where T : Exception
        {
            private readonly HandlerChain _parent;

            public OnExceptionExpression(HandlerChain parent)
            {
                _parent = parent;
            }

            public void Retry()
            {
                ContinueWith(new RetryNowContinuation());
            }

            public void Requeue()
            {
                ContinueWith(new RequeueContinuation());
            }

            public void MoveToErrorQueue()
            {
                _parent.ErrorHandlers.Add(new MoveToErrorQueueHandler<T>());
            }

            public void RetryLater(TimeSpan delay)
            {
                ContinueWith(new DelayedRetryContinuation(delay));
            }

            public void ContinueWith(IContinuation continuation)
            {
                var handler = new ErrorHandler();
                handler.AddCondition(new ExceptionTypeMatch<T>());
                handler.Continuation = continuation;

                _parent.ErrorHandlers.Add(handler);
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