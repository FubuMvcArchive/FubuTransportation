using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Runtime
{
    public class TransportActivator : IActivator
    {
        private readonly ChannelGraph _graph;
        private readonly IServiceLocator _services;
        private readonly ISubscriptionGateway _subscriptionGateway;

        public TransportActivator(ChannelGraph graph, IServiceLocator services, ISubscriptionGateway subscriptionGateway)
        {
            _graph = graph;
            _services = services;
            _subscriptionGateway = subscriptionGateway;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _graph.ReadSettings(_services);
            _subscriptionGateway.Start();
        }
    }
}