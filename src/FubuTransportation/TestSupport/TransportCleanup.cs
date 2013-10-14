using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
}