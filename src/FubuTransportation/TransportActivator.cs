using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Configuration;
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
            /*
             * 1.) take in the ChannelGraph
             * 2.) take in the IServiceLocator
             * 3.) ChannelGraph.FindSettings(services)
             * 4.) for each ITransport, Start(graph)
             * 5.) for each listening ChannelNode, StartReceiving(IReceiver)
             * 
             */
            throw new NotImplementedException();
        }
    }
}