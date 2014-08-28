using System;
using System.Collections.Generic;

namespace FubuTransportation.Monitoring
{
    public class HealthAndAssignmentRouter
    {
        private readonly IList<ITransportPeer> _allPeers = new List<ITransportPeer>(); 

        public HealthAndAssignmentRouter(IPersistentTasks tasks, IEnumerable<Uri> subjects, ITransportPeer local, IEnumerable<ITransportPeer> others)
        {
            _allPeers.Add(local);
            _allPeers.AddRange(others);
        }

        public IEnumerable<Uri> Unknowns { get; private set; }
        public IEnumerable<Uri> LocallyOwned { get; private set; }
        public IEnumerable<Uri> Unowned { get; private set; }
 


        // already know about the things that are owned nowhere
    }
}