using FubuCore.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class RequeueContinuation : IContinuation
    {
        public void Execute(Envelope envelope, ContinuationContext context)
        {
            envelope.Callback.Requeue();
        }
    }
}