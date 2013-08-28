using FubuCore.Logging;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainExecution : IContinuation
    {
        private readonly IMessageInvoker _invoker;
        private readonly HandlerChain _chain;

        public ChainExecution(IMessageInvoker invoker, HandlerChain chain)
        {
            _invoker = invoker;
            _chain = chain;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            _invoker.ExecuteChain(envelope, _chain);
        }

        public IMessageInvoker Invoker
        {
            get { return _invoker; }
        }

        public HandlerChain Chain
        {
            get { return _chain; }
        }
    }
}