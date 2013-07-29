using System;
using FubuCore.Binding;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;

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
            var inputType = envelope.Message.GetType();

            if (envelope.Message.GetType() == typeof (object[]))
            {
                var chain = _graph.ChainFor(typeof (object[]));
                executeChain(envelope, typeof (object[]), chain, envelope.Message);
            }
            else
            {
                var chain = _graph.ChainFor(inputType);
                if (chain == null)
                {
                    // TODO -- got to do something here for error handling or broadcasting
                    throw new NotImplementedException();
                }

                executeChain(envelope, inputType, chain, envelope.Message);
            }
        }

        private void executeChain(Envelope envelope, Type inputType, HandlerChain chain, object message)
        {
            var request = new InMemoryFubuRequest();
            request.Set(inputType, message);

            var outgoing = new OutgoingMessages();

            ServiceArguments args = new ServiceArguments()
                .With<IFubuRequest>(request)
                .With<IOutgoingMessages>(outgoing);

            IActionBehavior behavior = _factory.BuildBehavior(args, chain.UniqueId);

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