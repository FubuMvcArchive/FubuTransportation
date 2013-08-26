using System;
using System.Collections.Generic;
using System.Net;
using FubuTransportation.Runtime;
using Rhino.Queues;

namespace FubuTransportation.RhinoQueues
{
    public interface IPersistentQueues : IDisposable
    {
        IQueueManager ManagerFor(IPEndPoint endpoint);
        void Start(IEnumerable<RhinoUri> uriList);

        void CreateQueue(RhinoUri uri);
        IEnumerable<Envelope> ReplayDelayed(DateTime currentTime);
    }
}