namespace FubuTransportation.Runtime.Invocation
{
    public class ChainExecutionEnvelopeHandler : IEnvelopeHandler
    {
        private readonly IMessageInvoker _invoker;

        public ChainExecutionEnvelopeHandler(IMessageInvoker invoker)
        {
            _invoker = invoker;
        }

        public IContinuation Handle(Envelope envelope)
        {
            var chain = _invoker.FindChain(envelope);
            return chain == null ? null : new ChainExecution(_invoker, chain);
        }
    }
}