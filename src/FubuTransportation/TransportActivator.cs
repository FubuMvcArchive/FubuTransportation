using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation
{
    public class TransportActivator : IActivator
    {
        private readonly IEnumerable<ITransport> _transports;
        private readonly IReceiver _receiver;
        private readonly ChannelGraph _graph;
        private readonly IServiceLocator _services;

        public TransportActivator(IEnumerable<ITransport> transports, IReceiver receiver, ChannelGraph graph, IServiceLocator services)
        {
            _transports = transports;
            _receiver = receiver;
            _graph = graph;
            _services = services;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _graph.ReadSettings(_services);
            _transports.Each(x => x.OpenChannels(_graph));

            _graph.StartReceiving(_receiver);
        }
    }
}