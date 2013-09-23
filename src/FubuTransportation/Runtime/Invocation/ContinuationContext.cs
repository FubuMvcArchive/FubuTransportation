using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Runtime.Cascading;

namespace FubuTransportation.Runtime.Invocation
{
    public class ContinuationContext
    {
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;
        private readonly IChainInvoker _invoker;
        private readonly IOutgoingSender _outgoing;

        public ContinuationContext(ILogger logger, ISystemTime systemTime, IChainInvoker invoker, IOutgoingSender outgoing)
        {
            _logger = logger;
            _systemTime = systemTime;
            _invoker = invoker;
            _outgoing = outgoing;
        }

        // virtual for testing, setter to avoid bi-directional dependency problems
        public virtual IHandlerPipeline Pipeline { get; set; }

        public IOutgoingSender Outgoing
        {
            get { return _outgoing; }
        }

        public ILogger Logger
        {
            get { return _logger; }
        }

        public ISystemTime SystemTime
        {
            get { return _systemTime; }
        }

        public IChainInvoker Invoker
        {
            get { return _invoker; }
        }
    }
}