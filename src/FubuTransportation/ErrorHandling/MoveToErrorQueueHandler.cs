using System;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class MoveToErrorQueueHandler<T> : IErrorHandler where T : Exception
    {
        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            if (ex is T) return new MoveToErrorQueue(ex);

            return null;
        }
    }
}