using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuTransportation.Monitoring;

namespace FubuTransportation.Testing.Monitoring
{
    public class FakePersistentTaskAgent : IPersistentTaskAgent
    {
        public FakePersistentTaskAgent(Uri subject)
        {
            Subject = subject;
        }

        public Uri Subject { get; private set; }
        public bool IsActive { get; private set; }
        public Task<HealthStatus> AssertAvailable()
        {
            throw new NotImplementedException();
        }

        public Task<OwnershipStatus> Activate()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Deactivate()
        {
            throw new NotImplementedException();
        }

        public Task<ITransportPeer> AssignOwner(IEnumerable<ITransportPeer> peers)
        {
            SelectedPeer = peers.FirstOrDefault(x => x.NodeId == PeerIdToSelect);
            return SelectedPeer.TakeOwnership(Subject).ContinueWith(t => SelectedPeer);
        }

        public string PeerIdToSelect;
        public ITransportPeer SelectedPeer;
    }
}