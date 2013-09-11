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
            if (envelope.Message == null)
            {
                context.Logger.Error(envelope.CorrelationId, "Error trying to execute a message of type " + envelope.Headers[Envelope.MessageTypeKey], _exception);
            }
            else
            {
                context.Logger.Error(envelope.CorrelationId, envelope.Message.ToString(), _exception);
            }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
    }
}