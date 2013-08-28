using FubuCore.Logging;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainInvoker : IChainInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly ILogger _logger;
        private readonly IEnvelopeSender _sender;

        public ChainInvoker(IServiceFactory factory, HandlerGraph graph, ILogger logger, IEnvelopeSender sender)
        {
            _factory = factory;
            _graph = graph;
            _logger = logger;
            _sender = sender;
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

            var args = new InvocationContext(envelope);
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


        public IInvocationContext ExecuteChain(Envelope envelope, HandlerChain chain)
        {
            using (new ChainExecutionWatcher(_logger, chain, envelope))
            {
                var context = new InvocationContext(envelope);
                var behavior = _factory.BuildBehavior(context, chain.UniqueId);

                behavior.Invoke();

                return context;
            }
        }
    }
}