using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FubuTransportation.Monitoring
{

    // TODO -- try to clean up unused members
    // Rename?

    public interface ITransportPeer
    {
        Task<OwnershipStatus> TakeOwnership(Uri subject);

        // TODO -- go to async/await?
        Task<TaskHealthResponse> CheckStatusOfOwnedTasks();

        void RemoveOwnershipFromNode(IEnumerable<Uri> subjects);

        IEnumerable<Uri> CurrentlyOwnedSubjects();

        string NodeId { get; }
        string MachineName { get; }
        Uri ControlChannel { get; }
        Task<bool> Deactivate(Uri subject);
    }
}