using System;
using FubuCore.Binding;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using System.Linq;

namespace FubuTransportation.Runtime
{
    public class MessageInvoker : IMessageInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;

        public MessageInvoker(IServiceFactory factory, HandlerGraph graph)
        {
            _factory = factory;
            _graph = graph;
        }

        public void Invoke(Envelope envelope)
        {
            if (envelope.Messages.Length == 1)
            {
                var inputType = envelope.Messages.Single().GetType();
                var chain = _graph.ChainFor(inputType);
                if (chain == null)
                {
                    // TODO -- got to do something here for error handling or broadcasting
                    throw new NotImplementedException();
                }
                else
                {
                    executeChain(envelope, inputType, chain, envelope.Messages.Single());
                }
            }
            else
            {
                var chain = _graph.ChainFor(typeof (object[]));
                executeChain(envelope, typeof(object[]), chain, envelope.Messages);
            }
        }

        private void executeChain(Envelope envelope, Type inputType, HandlerChain chain, object message)
        {
            var request = new InMemoryFubuRequest();
            request.Set(inputType, message);

            var outgoing = new OutgoingMessages();

            var args = new ServiceArguments()
                .With<IFubuRequest>(request)
                .With<IOutgoingMessages>(outgoing);

            var behavior = _factory.BuildBehavior(args, chain.UniqueId);

            try
            {
                behavior.Invoke();
                envelope.Callback.MarkSuccessful();
            }
            catch (Exception ex)
            {
                // TODO -- um, do something here
                throw;
                //envelope.MarkFailed();
            }
        }
    }
}