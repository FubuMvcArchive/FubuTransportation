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
    public class ChainInvoker : IChainInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IEnvelopeSender _sender;

        public ChainInvoker(IServiceFactory factory, HandlerGraph graph, IEnvelopeSerializer serializer, ILogger logger, IEnvelopeSender sender)
        {
            _factory = factory;
            _graph = graph;
            _serializer = serializer;
            _logger = logger;
            _sender = sender;
        }

        public IEnvelopeSerializer Serializer
        {
            get { return _serializer; }
        }

        public void Invoke(Envelope envelope)
        {
            var chain = FindChain(envelope);

            ExecuteChain(envelope, chain);
        }

        public void InvokeNow<T>(T message)
        {
            // TODO -- log failures, but throw the exception
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


        public void ExecuteChain(Envelope envelope, HandlerChain chain)
        {
            using (new ChainExecutionWatcher(_logger, chain, envelope))
            {
                var args = new HandlerArguments(envelope);
                var behavior = _factory.BuildBehavior(args, chain.UniqueId);

                try
                {
                    behavior.Invoke();
                    sendCascadingMessages(envelope, args);

                    envelope.Callback.MarkSuccessful();
                    _logger.InfoMessage(() => new MessageSuccessful {Envelope = envelope.ToToken()});
                }
                catch (Exception ex)
                {
                    logFailure(envelope, ex);
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


        private void logFailure(Envelope envelope, Exception ex)
        {
            envelope.Callback.MarkFailed();
            _logger.InfoMessage(() => new MessageFailed {Envelope = envelope.ToToken(), Exception = ex});
            _logger.Error(envelope.CorrelationId, ex);
        }
    }

    public class NoHandlerException : Exception
    {
        public NoHandlerException(Type messageType) : base("No registered handler for message type " + messageType.FullName)
        {
        }
    }

}