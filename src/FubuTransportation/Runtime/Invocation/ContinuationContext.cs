using FubuCore.Dates;
using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class ContinuationContext
    {
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;
        private readonly IChainInvoker _invoker;

        public ContinuationContext(ILogger logger, ISystemTime systemTime, IChainInvoker invoker)
        {
            _logger = logger;
            _systemTime = systemTime;
            _invoker = invoker;
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