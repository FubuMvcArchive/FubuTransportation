using System;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainFailureContinuation : IContinuation
    {
        private readonly Exception _exception;

        public ChainFailureContinuation(Exception exception)
        {
            _exception = exception;
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            envelope.Callback.MarkFailed();
            context.Logger.InfoMessage(() => new MessageFailed {Envelope = envelope.ToToken(), Exception = _exception});
            context.Logger.Error(envelope.CorrelationId, _exception);
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}