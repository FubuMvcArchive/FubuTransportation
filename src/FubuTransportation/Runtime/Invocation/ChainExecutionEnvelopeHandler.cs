using System;
using FubuTransportation.Runtime.Cascading;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainExecutionEnvelopeHandler : IEnvelopeHandler
    {
        private readonly IChainInvoker _invoker;
        private readonly IOutgoingSender _sender;

        public ChainExecutionEnvelopeHandler(IChainInvoker invoker, IOutgoingSender sender)
        {
            _invoker = invoker;
            _sender = sender;
        }

        public IContinuation Handle(Envelope envelope)
        {
            var chain = _invoker.FindChain(envelope);
            if (chain == null)
            {
                return null;
            }

            try
            {
                var context = _invoker.ExecuteChain(envelope, chain);
                return context.Continuation ?? new ChainSuccessContinuation(_sender, context);

            }
            catch (Exception ex)
            {
                // TODO -- might be nice to capture the Chain
                return new ChainFailureContinuation(ex);
            }
        }
    }
}