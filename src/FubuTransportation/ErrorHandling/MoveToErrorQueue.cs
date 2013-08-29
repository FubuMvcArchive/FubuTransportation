using System;
using FubuCore.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class MoveToErrorQueue : IContinuation
    {
        private readonly Exception _exception;

        public MoveToErrorQueue(Exception exception)
        {
            _exception = exception;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            var report = new ErrorReport(envelope, _exception);
            envelope.Callback.MoveToErrors(report);
        }
    }
}