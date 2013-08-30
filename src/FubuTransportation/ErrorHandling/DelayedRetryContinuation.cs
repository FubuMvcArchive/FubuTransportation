using System;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class DelayedRetryContinuation : IContinuation
    {
        private readonly TimeSpan _delay;

        public DelayedRetryContinuation(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            envelope.Callback.MoveToDelayedUntil(context.SystemTime.UtcNow().Add(_delay));
        }
    }
}