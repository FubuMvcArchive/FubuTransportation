using System.Collections.Generic;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerRepository
    {
        IEnumerable<ITransportPeer> AllPeers();
        IEnumerable<ITransportPeer> AllOwners();
    }
}