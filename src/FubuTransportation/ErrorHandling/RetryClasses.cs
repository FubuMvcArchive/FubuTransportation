using System;
using System.Linq.Expressions;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    /*
     * TODO's
     * Put ReceivedTime on Envelope in Receiver?
     * Need some way of configuring retries
     * 
     * 
     * 
     */

    public class ExceptionHandlerBehavior : IActionBehavior
    {
        public ExceptionHandlerBehavior(HandlerChain chain, Envelope envelope)
        {
        }

        public void Invoke()
        {
            // log the exception no matter what!
            throw new NotImplementedException();
        }

        public void InvokePartial()
        {
            throw new NotImplementedException();
        }
    }

    public class DelayedRetryContinuation : IContinuation
    {
        private readonly TimeSpan _delay;

        public DelayedRetryContinuation(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}