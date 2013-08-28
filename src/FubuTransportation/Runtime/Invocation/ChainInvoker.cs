using System;
using System.Collections.Generic;
using FubuCore.Logging;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Serializers;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainInvoker : IChainInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly ILogger _logger;
        private readonly IEnvelopeSender _sender;
        private readonly IEnvelopeSerializer _serializer;

        public ChainInvoker(IServiceFactory factory, HandlerGraph graph, IEnvelopeSerializer serializer, ILogger logger,
                            IEnvelopeSender sender)
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
                throw new NoHandlerException(typeof (T));
            }

            var args = new HandlerArguments(envelope);
            var behavior = _factory.BuildBehavior(args, chain.UniqueId);
            behavior.Invoke();

            _sender.SendOutgoingMessages(envelope, args.OutgoingMessages());
        }

        public virtual HandlerChain FindChain(Envelope envelope)
        {
            var messageType = envelope.Message.GetType();

            // TODO -- going to get rid of this in favor of a formal "Batch" concept
            return _graph.ChainFor(messageType == typeof (object[]) ? typeof (object[]) : messageType);
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
                    _sender.SendOutgoingMessages(envelope, args.OutgoingMessages());

                    envelope.Callback.MarkSuccessful();
                    _logger.InfoMessage(() => new MessageSuccessful {Envelope = envelope.ToToken()});
                }
                catch (Exception ex)
                {
                    logFailure(envelope, ex);
                }
            }
        }

        private void logFailure(Envelope envelope, Exception ex)
        {
            envelope.Callback.MarkFailed();
            _logger.InfoMessage(() => new MessageFailed {Envelope = envelope.ToToken(), Exception = ex});
            _logger.Error(envelope.CorrelationId, ex);
        }
    }

    public class ChainSuccessContinuation : IContinuation
    {
        private readonly IEnvelopeSender _sender;
        private readonly IInvocationContext _context;

        public ChainSuccessContinuation(IEnvelopeSender sender, IInvocationContext context)
        {
            _sender = sender;
            _context = context;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            _sender.SendOutgoingMessages(envelope, _context.OutgoingMessages());

            envelope.Callback.MarkSuccessful();
            logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope.ToToken() });
        }
    }

    public class ChainFailureContinuation : IContinuation
    {
        private readonly Exception _ex;

        public ChainFailureContinuation(Exception ex)
        {
            _ex = ex;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            envelope.Callback.MarkFailed();
            logger.InfoMessage(() => new MessageFailed { Envelope = envelope.ToToken(), Exception = _ex });
            logger.Error(envelope.CorrelationId, _ex);
        }
    }

    public class NoHandlerException : Exception
    {
        public NoHandlerException(Type messageType)
            : base("No registered handler for message type " + messageType.FullName)
        {
        }
    }
}