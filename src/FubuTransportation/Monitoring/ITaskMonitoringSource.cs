using System;
using System.Collections.Generic;

namespace FubuTransportation.Monitoring
{
    public interface ITaskMonitoringSource
    {
        // TODO -- going to be a Task later
        IEnumerable<ITransportPeer> BuildPeers();

        IEnumerable<Uri> LocallyOwnedTasksAccordingToPersistence();

        IPersistentTaskAgent BuildAgentFor(IPersistentTask task);
    }
}