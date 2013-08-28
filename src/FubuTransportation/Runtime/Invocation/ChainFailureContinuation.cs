using System;
using FubuCore.Logging;
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

        public void Execute(Envelope envelope, ILogger logger)
        {
            envelope.Callback.MarkFailed();
            logger.InfoMessage(() => new MessageFailed { Envelope = envelope.ToToken(), Exception = _exception });
            logger.Error(envelope.CorrelationId, _exception);
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}