using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Runtime;

namespace FubuTransportation
{
    public class TransportActivator : IActivator
    {
        private readonly IEnumerable<ITransport> _transports;
        private readonly IReceiver _receiver;

        public TransportActivator(IEnumerable<ITransport> transports, IReceiver receiver)
        {
            _transports = transports;
            _receiver = receiver;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _transports.Each(x => x.StartReceiving(_receiver));
        }
    }
}