using System;
using System.Collections.Generic;
using FubuCore.Logging;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSerializer _serializer;
        private readonly ContinuationContext _context;
        private readonly IList<IEnvelopeHandler> _handlers = new List<IEnvelopeHandler>();

        public HandlerPipeline(IEnvelopeSerializer serializer, ContinuationContext context, IEnumerable<IEnvelopeHandler> handlers)
        {
            _serializer = serializer;
            _context = context;
            _handlers.AddRange(handlers);
        }

        public void Invoke(Envelope envelope)
        {
            envelope.UseSerializer(_serializer);

            _context.Logger.InfoMessage(() => new EnvelopeReceived { Envelope = envelope.ToToken() });

            var continuation = FindContinuation(envelope);

            // Harden this!!!!!  No exceptions get through, ever.
            continuation.Execute(envelope, _context);
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