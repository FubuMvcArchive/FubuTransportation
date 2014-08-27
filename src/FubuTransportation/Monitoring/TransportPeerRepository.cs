using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class TransportPeerRepository : ITransportPeerRepository
    {
        private readonly IServiceBus _serviceBus;
        private readonly ISubscriptionRepository _subscriptions;

        public TransportPeerRepository(IServiceBus serviceBus, ISubscriptionRepository subscriptions)
        {
            _serviceBus = serviceBus;
            _subscriptions = subscriptions;
        }

        public bool HasAnyPeers()
        {
            return _subscriptions.FindPeers().Any();
        }

        private TransportPeer toPeer(TransportNode node)
        {
            return new TransportPeer(node, _subscriptions, _serviceBus);
        }

        public IEnumerable<ITransportPeer> AllPeers()
        {
            return _subscriptions.FindPeers().Select(toPeer).ToArray();
        }

        public IEnumerable<ITransportPeer> AllOwners()
        {
            return _subscriptions.FindPeers().Where(x => x.OwnedTasks.Any())
                .Select(toPeer).ToArray();
        }
    }
}