using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    // TODO -- kill this and fold its builder functionality
    // into PersistentTaskController
    public class TransportPeerRepository : ITransportPeerRepository
    {
        private readonly HealthMonitoringSettings _settings;
        private readonly ILogger _logger;
        private readonly IServiceBus _serviceBus;
        private readonly ISubscriptionRepository _subscriptions;

        public TransportPeerRepository(HealthMonitoringSettings settings, ILogger logger, IServiceBus serviceBus, ISubscriptionRepository subscriptions)
        {
            _settings = settings;
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
            return new TransportPeer(_settings, node, _subscriptions, _serviceBus, _logger);
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
            _subscriptions.AddOwnershipToThisNode(subject);
        }

        public void RecordOwnershipToThisNode(IEnumerable<Uri> subjects)
        {
            _subscriptions.AddOwnershipToThisNode(subjects);
        }

        public TransportNode LocalNode()
        {
            return _subscriptions.FindLocal();
        }

        public void RemoveOwnershipFromThisNode(Uri subject)
        {
            _subscriptions.RemoveOwnershipFromThisNode(subject);
        }
    }
}