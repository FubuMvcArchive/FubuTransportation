using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class TransportPeerRepository : ITransportPeerRepository
    {
        private readonly ILogger _logger;
        private readonly IServiceBus _serviceBus;
        private readonly ISubscriptionRepository _subscriptions;

        public TransportPeerRepository(ILogger logger, IServiceBus serviceBus, ISubscriptionRepository subscriptions)
        {
            _logger = logger;
            _serviceBus = serviceBus;
            _subscriptions = subscriptions;
        }

        public bool HasAnyPeers()
        {
            return _subscriptions.FindPeers().Any();
        }

        private TransportPeer toPeer(TransportNode node)
        {
            return new TransportPeer(node, _subscriptions, _serviceBus, _logger);
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

        public void RecordOwnershipToThisNode(Uri subject)
        {
            throw new NotImplementedException();
        }

        public void RecordOwnershipToThisNode(IEnumerable<Uri> subjects)
        {
            throw new NotImplementedException();
        }

        public TransportNode LocalNode()
        {
            return _subscriptions.FindLocal();
        }
    }
}