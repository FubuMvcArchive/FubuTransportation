using System;
using System.Runtime.Serialization;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public class MessageInvoker : IMessageInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IEnvelopeSender _sender;
        private readonly ISystemTime _systemTime;

        public MessageInvoker(IServiceFactory factory, HandlerGraph graph, IEnvelopeSerializer serializer, ILogger logger, IEnvelopeSender sender, ISystemTime systemTime)
        {
            _factory = factory;
            _graph = graph;
            _serializer = serializer;
            _logger = logger;
            _sender = sender;
            _systemTime = systemTime;
        }

        public IEnvelopeSerializer Serializer
        {
            get { return _serializer; }
        }

        public void Invoke(Envelope envelope, IMessageCallback callback)
        {
            if (envelope.IsDelayed(_systemTime.UtcNow()))
            {
                try
                {
                    callback.MoveToDelayed();
                    _logger.InfoMessage(() => new DelayedEnvelopeReceived{Envelope = envelope});
                }
                catch (Exception e)
                {
                    _logger.Error(envelope.CorrelationId, "Failed to move delayed message to the delayed message queue", e);
                }

                return;
            }

            if (envelope.Message == null)
            {
                _serializer.Deserialize(envelope);
            }

            _logger.InfoMessage(() => new EnvelopeReceived{Envelope = envelope});

            // Do nothing for responses other than kick out the EnvelopeReceived.
            if (envelope.ResponseId.IsNotEmpty())
            {
                _logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope });
                callback.MarkSuccessful();
                return;
            }

            var chain = FindChain(envelope);
            if (chain == null)
            {
                _logger.InfoMessage(() => new NoHandlerForMessage { Envelope = envelope });
                callback.MarkSuccessful();
                return;
            }

            ExecuteChain(envelope, chain, callback);


        }

        public void InvokeNow<T>(T message)
        {
            var envelope = new Envelope {Message = message};
            var chain = FindChain(envelope);
            if (chain == null)
            {
                throw new NoHandlerException(typeof(T));
            }

            var args = new HandlerArguments(envelope);
            var behavior = _factory.BuildBehavior(args, chain.UniqueId);
            behavior.Invoke();
            sendCascadingMessages(envelope, args);
        }

        public virtual HandlerChain FindChain(Envelope envelope)
        {
            var messageType = envelope.Message.GetType();

            // TODO -- going to get rid of this in favor of a formal "Batch" concept
            return _graph.ChainFor(messageType == typeof(object[]) ? typeof(object[]) : messageType);
        }


        public virtual void ExecuteChain(Envelope envelope, HandlerChain chain, IMessageCallback callback)
        {
            using (new ChainExecutionWatcher(_logger, chain, envelope))
            {
                var args = new HandlerArguments(envelope);
                var behavior = _factory.BuildBehavior(args, chain.UniqueId);

                try
                {
                    behavior.Invoke();
                    sendCascadingMessages(envelope, args);

                    callback.MarkSuccessful();
                    _logger.InfoMessage(() => new MessageSuccessful {Envelope = envelope});
                }
                catch (Exception ex)
                {
                    logFailure(envelope, callback, ex);
                }
            }
        }

        private void sendCascadingMessages(Envelope envelope, HandlerArguments args)
        {
            args.OutgoingMessages().Each(o => {
                var child = envelope.ForResponse(o);
                _sender.Send(child);
            });
        }


        private void logFailure(Envelope envelope, IMessageCallback callback, Exception ex)
        {
            callback.MarkFailed();
            _logger.InfoMessage(() => new MessageFailed {Envelope = envelope, Exception = ex});
            _logger.Error(envelope.CorrelationId, ex);
        }
    }

    [Serializable]
    public class NoHandlerException : Exception
    {
        public NoHandlerException(Type messageType)
            :base("No handler for messsage type " + messageType.FullName)
        {
            
        }

        protected NoHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}