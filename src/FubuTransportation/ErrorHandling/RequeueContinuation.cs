using FubuCore.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public void Execute(Envelope envelope, ILogger logger)
        {
            envelope.Callback.Requeue();
        }
    }
}