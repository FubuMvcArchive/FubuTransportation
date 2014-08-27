using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeer
    {
        Task<OwnershipStatus> TakeOwnership(Uri subject);
        Task<TaskHealthResponse> CheckStatusOfOwnedTasks();

        IEnumerable<Uri> CurrentlyOwnedSubjects();

        string NodeId { get; }
        string MachineName { get; }
        IEnumerable<Uri> ReplyAddresses { get; }
    }
}