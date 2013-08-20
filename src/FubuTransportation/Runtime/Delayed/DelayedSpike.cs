using System;

namespace FubuTransportation.Runtime.Delayed
{
    public interface IDelayedEnvelopeStorage
    {
        void Store(DateTime requestedTime, Envelope envelope);
        void Dequeue(DateTime now, Action<Envelope> callback);

        // TODO -- do something for diagnostics later
    }
}