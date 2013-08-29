using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public void InvokePartial()
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorReport
    {
        public object Message { get; set; }

        public ErrorReport(object message, Exception ex)
        {
            Message = message;
            ExceptionText = ex.ToString();
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().FullName;
            Explanation = "Exception Detected";
        }

        public string Explanation { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionText { get; set; }
    }

    // Make this stateless
    public interface IErrorHandler
    {
        IContinuation DetermineContinuation(Envelope envelope, Exception ex);
    }

    public class ErrorHandler : IErrorHandler, IErrorCondition
    {
        private readonly IList<IErrorCondition> _conditions = new List<IErrorCondition>(); 

        public IContinuation Continuation = new MoveToErrorQueue();

        public void AddCondition(IErrorCondition condition)
        {
            _conditions.Add(condition);
        }

        public IEnumerable<IErrorCondition> Conditions
        {
            get { return _conditions; }
        }

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return Matches(envelope, ex) ? Continuation : null;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public interface IErrorCondition
    {
        bool Matches(Envelope envelope, Exception ex);
    }

    public class Always : IErrorCondition
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            return true;
        }
    }

    public class ExceptionType<T> : IErrorCondition where T : Exception
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageContains : IErrorCondition
    {
        public MessageContains(string text)
        {
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public class RequeueContinuation : IContinuation
    {
        public void Execute(Envelope envelope, ILogger logger)
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

    public class MoveToErrorQueue : IContinuation
    {
        public void Execute(Envelope envelope, ILogger logger)
        {
            throw new NotImplementedException();
        }
    }


}