namespace FubuTransportation.Runtime.Invocation
{
    public class ChainExecutionEnvelopeHandler : IEnvelopeHandler
    {
        private readonly IChainInvoker _invoker;

        public ChainExecutionEnvelopeHandler(IChainInvoker invoker)
        {
            _invoker = invoker;
        }

        public IContinuation Handle(Envelope envelope)
        {
            var chain = _invoker.FindChain(envelope);
            if (chain == null)
            {
                return null;
            }

            

            return new ChainExecution(_invoker, chain);
        }
    }
}