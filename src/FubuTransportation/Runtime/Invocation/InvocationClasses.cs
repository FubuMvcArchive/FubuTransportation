using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IContinuation
    {
        void Execute(Envelope envelope, IMessageCallback callback);
    }

    // Another handler for no subscriber rules!
    public interface IEnvelopeHandler
    {
        IContinuation Handle(Envelope envelope);
    }

    public interface IHandlerPipeline
    {
        void Invoke(Envelope envelope, IMessageCallback callback);
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

        public void Invoke(Envelope envelope, IMessageCallback callback)
        {
            // TODO -- blow up if envelope callback is null

            // TODO -- log received

            if (envelope.Message == null)
            {
                _serializer.Deserialize(envelope);
            }

            var continuation = FindContinuation(envelope);
            continuation.Execute(envelope, callback);
        }

        public IContinuation FindContinuation(Envelope envelope)
        {
            foreach (IEnvelopeHandler handler in _handlers)
            {
                var continuation = handler.Handle(envelope);
                if (continuation != null)
                {
                    // TODO log something
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