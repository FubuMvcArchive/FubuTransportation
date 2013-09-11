using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class RetryNowContinuation : IContinuation
    {
        public void Execute(Envelope envelope, ContinuationContext context)
        {
            context.Pipeline.Invoke(envelope);
        }
    }
}