using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Runtime;

namespace FubuTransportation.TestSupport
{
    public class TransportCleanup : Bottles.Services.Messaging.IListener<ClearAllTransports>
    {
        private readonly IEnumerable<ITransport> _transports;

        public TransportCleanup(IEnumerable<ITransport> transports)
        {
            _transports = transports;
        }

        public void Receive(ClearAllTransports message)
        {
            // Debug tracing will get written out into the Storyteller 
            // output
            ClearAll();
        }

        public void ClearAll()
        {
            _transports.Each(x => {
                Debug.WriteLine("Clearing up all messages on " + x);
                x.ClearAll();
            });
        }
    }

    public class ClearAllTransports { }

    public class TransportCleanupActivator : IActivator
    {
        private readonly TransportCleanup _cleanup;

        public TransportCleanupActivator(TransportCleanup cleanup)
        {
            _cleanup = cleanup;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            log.Trace("Adding TransportCleanup to the Bottles EventAggregator");
            Bottles.Services.Messaging.EventAggregator.Messaging.AddListener(_cleanup);
        }
    }
}