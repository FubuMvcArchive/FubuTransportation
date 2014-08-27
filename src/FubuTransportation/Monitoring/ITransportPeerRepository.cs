using System;
using System.Collections.Generic;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerRepository
    {
        IEnumerable<ITransportPeer> AllPeers();
        IEnumerable<ITransportPeer> AllOwners();

        void AlterThisNode(Action<TransportNode> alteration);
    }
}