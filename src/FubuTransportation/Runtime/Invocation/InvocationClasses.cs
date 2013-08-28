﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IContinuation
    {
        void Execute(Envelope envelope);
    }

    // Another handler for no subscriber rules!
    public interface IEnvelopeHandler
    {
        IContinuation Handle(Envelope envelope);
    }

    public interface IHandlerPipeline
    {
        void Invoke(Envelope envelope);
    }

    public class HandlerPipeline : IHandlerPipeline
    {
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IList<IEnvelopeHandler> _handlers = new List<IEnvelopeHandler>();

        public HandlerPipeline(IEnvelopeSerializer serializer, ILogger logger, IEnumerable<IEnvelopeHandler> handlers)
        {
            _serializer = serializer;
            _logger = logger;
            _handlers.AddRange(handlers);
        }

        public void Invoke(Envelope envelope)
        {
            envelope.UseSerializer(_serializer);

            _logger.InfoMessage(() => new EnvelopeReceived { Envelope = envelope.ToToken() });

            var continuation = FindContinuation(envelope);
            continuation.Execute(envelope);
        }

        // virtual for testing as usual
        public virtual IContinuation FindContinuation(Envelope envelope)
        {
            foreach (var handler in _handlers)
            {
                var continuation = handler.Handle(envelope);
                if (continuation != null)
                {
                    _logger.DebugMessage(() => new EnvelopeContinuationChosen
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
    }

    public class DelayedMessageHandler
    {
        
    }

}