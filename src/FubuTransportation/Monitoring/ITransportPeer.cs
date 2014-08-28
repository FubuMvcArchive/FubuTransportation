using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeer
    {
        Task<OwnershipStatus> TakeOwnership(Uri subject);

        // TODO -- go to async/await?
        Task<TaskHealthResponse> CheckStatusOfOwnedTasks();

        IEnumerable<Uri> CurrentlyOwnedSubjects();

        string NodeId { get; }
        string MachineName { get; }
        Uri ControlChannel { get; }
        Task Deactivate(Uri subject);
    }
}