using System;
using System.Collections.Generic;
using System.Net;
using FubuTransportation.Runtime;
using LightningQueues;

namespace FubuTransportation.LightningQueues
{
    public interface IPersistentQueues : IDisposable
    {
        void ClearAll();
        IQueueManager ManagerFor(IPEndPoint endpoint, bool incoming);
        IQueueManager ManagerForReply();
        void Start(IEnumerable<LightningUri> uriList);

        void CreateQueue(LightningUri uri);
        IEnumerable<EnvelopeToken> ReplayDelayed(DateTime currentTime);
    }
}