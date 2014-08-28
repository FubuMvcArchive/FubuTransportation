using System;
using System.Collections.Generic;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerRepository
    {
        IEnumerable<ITransportPeer> AllPeers();
        IEnumerable<ITransportPeer> AllOwners();

        void RecordOwnershipToThisNode(Uri subject);
        void RecordOwnershipToThisNode(IEnumerable<Uri> subjects);
        TransportNode LocalNode();
    }
}