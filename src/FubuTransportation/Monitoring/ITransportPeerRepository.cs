using System;
using System.Collections.Generic;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerRepository
    {
        IEnumerable<ITransportPeer> AllPeers();
        IEnumerable<ITransportPeer> AllOwners();

        void RecordOwnershipToThisNode(Uri subject);
        void RecordOwnershipToThisNode(IEnumerable<Uri> subjects);
    }
}