using System;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSerializer _serializer;
        private readonly ContinuationContext _context;
        private readonly IList<IEnvelopeHandler> _handlers = new List<IEnvelopeHandler>();

        public HandlerPipeline(IEnvelopeSerializer serializer, ContinuationContext context,
            IEnumerable<IEnvelopeHandler> handlers)
        {
            _serializer = serializer;
            _context = context;
            _handlers.AddRange(handlers);

            // needs to be available to continuations
            _context.Pipeline = this;
        }

        public void Invoke(Envelope envelope)
        {
            envelope.Attempts++; // needs to be done here.
            if (envelope.Message == null)
            {
                envelope.UseSerializer(_serializer);
            }

            _context.Logger.InfoMessage(() => new EnvelopeReceived {Envelope = envelope.ToToken()});

            var continuation = FindContinuation(envelope);

            try
            {
                continuation.Execute(envelope, _context);
            }
            catch (Exception e)
            {
                envelope.Callback.MarkFailed(); // TODO -- watch this one.
                _context.Logger.Error(envelope.CorrelationId,
                    "Failed while invoking message {0} with continuation {1}".ToFormat(envelope.Message ?? envelope,
                        continuation), e);
            }
        }

        // virtual for testing as usual
        public virtual IContinuation FindContinuation(Envelope envelope)
        {
            foreach (var handler in _handlers)
            {
                var continuation = handler.Handle(envelope);
                if (continuation != null)
                {
                    _context.Logger.DebugMessage(() => new EnvelopeContinuationChosen
                    {
                        ContinuationType = continuation.GetType(),
                        HandlerType = handler.GetType(),
                        Envelope = envelope.ToToken()
                    });

                    return continuation;
                }
            }

            throw new NotSupportedException();
        }

        public IList<IEnvelopeHandler> Handlers
        {
            get { return _handlers; }
        }
    }
}