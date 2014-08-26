using System.Collections.Generic;

namespace FubuTransportation.ScheduledJobs.Persistence
{
    public interface ISchedulePersistence
    {
        IEnumerable<JobStatusDTO> FindAll(string nodeName);
        IEnumerable<JobStatusDTO> FindAllActive(string nodeName);
        void Persist(IEnumerable<JobStatusDTO> statuses);
        void Persist(JobStatusDTO status);

        JobStatusDTO Find(string nodeName, string jobKey);
    }
}