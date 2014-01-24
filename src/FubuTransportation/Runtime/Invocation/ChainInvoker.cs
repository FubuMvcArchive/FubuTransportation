using System;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime.Cascading;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainInvoker : IChainInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly ILogger _logger;
        private readonly IOutgoingSender _sender;
        private readonly ISystemTime _systemTime;

        public ChainInvoker(IServiceFactory factory, HandlerGraph graph, ILogger logger, IOutgoingSender sender, ISystemTime systemTime)
        {
            _factory = factory;
            _graph = graph;
            _logger = logger;
            _sender = sender;
            _systemTime = systemTime;
        }

        public void Invoke(Envelope envelope)
        {
            var chain = FindChain(envelope);

            ExecuteChain(envelope, chain);
        }

        public void InvokeNow<T>(T message)
        {
            var envelope = new Envelope {Message = message};
            var chain = FindChain(envelope);
            if (chain == null)
            {
                throw new NoHandlerException(typeof (T));
            }

            IActionBehavior behavior = null;

            try
            {
                envelope.Callback = new InlineMessageCallback(message, _sender);

                var args = new InvocationContext(envelope, chain);
                behavior = _factory.BuildBehavior(args, chain.UniqueId);
                behavior.Invoke();

                var continuationContext = new ContinuationContext(_logger, _systemTime, this, _sender);
                var continuation = args.Continuation ?? new ChainSuccessContinuation(args);
                continuation.Execute(envelope, continuationContext);
            }
            catch (Exception e)
            {
                if (!envelope.ToToken().IsPollingJobRelated())
                {
                    _logger.Error("Failed while invoking message " + message, e);
                }
                throw;
            }
            finally
            {
                (behavior as IDisposable).CallIfNotNull(x => x.SafeDispose());
            }
        }

        public virtual HandlerChain FindChain(Envelope envelope)
        {
            var messageType = envelope.Message.GetType();
            return _graph.ChainFor(messageType);
        }


        public IInvocationContext ExecuteChain(Envelope envelope, HandlerChain chain)
        {
            using (new ChainExecutionWatcher(_logger, chain, envelope))
            {
                var context = new InvocationContext(envelope, chain);
                var behavior = _factory.BuildBehavior(context, chain.UniqueId);

                try
                {
                    behavior.Invoke();
                }
                finally
                {
                    (behavior as IDisposable).CallIfNotNull(x => x.SafeDispose());
                }

                return context;
            }
        }
    }

    public class InlineMessageCallback : IMessageCallback
    {
        private readonly object _message;
        private readonly IOutgoingSender _sender;

        public InlineMessageCallback(object message, IOutgoingSender sender)
        {
            _message = message;
            _sender = sender;
        }

        public void MarkSuccessful()
        {
        }

        public void MarkFailed()
        {
            
        }

        public void MoveToDelayedUntil(DateTime time)
        {
            _sender.Send(new Envelope
            {
                Message = _message,
                ExecutionTime = time
            });
        }

        public void MoveToErrors(ErrorReport report)
        {
            // TODO -- need a general way to log errors against an ITransport
        }

        public void Requeue()
        {
            _sender.Send(new Envelope
            {
                Message = _message
            });
        }
    }
}