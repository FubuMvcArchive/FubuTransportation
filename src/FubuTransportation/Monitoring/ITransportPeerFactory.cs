using System.Collections.Generic;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerFactory
    {
        // TODO -- going to be a Task later
        IEnumerable<ITransportPeer> BuildPeers();
    }
}