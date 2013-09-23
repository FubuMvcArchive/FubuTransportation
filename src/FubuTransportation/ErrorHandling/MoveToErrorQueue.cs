using System;
using System.ComponentModel;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    [Description("Moves the message envelope to the error queue for the current transport")]
    public class MoveToErrorQueue : IContinuation
    {
        private readonly Exception _exception;

        public MoveToErrorQueue(Exception exception)
        {
            _exception = exception;
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            var report = new ErrorReport(envelope, _exception);
            envelope.Callback.MoveToErrors(report);
        }
    }
}