using System;
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

    public interface IErrorCondition
    {
        bool Matches(Envelope envelope, Exception ex);
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