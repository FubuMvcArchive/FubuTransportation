using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuTransportation.ScheduledJobs
{
    public class InMemorySchedulePersistence : ISchedulePersistence
    {
        private readonly Cache<string, JobStatusDTO> _statusCache = new Cache<string, JobStatusDTO>(); 

        public IEnumerable<JobStatusDTO> FindAllActive(string nodeName)
        {
            return _statusCache.Where(x => x.NodeName == nodeName && x.Status != JobExecutionStatus.Inactive);
        }

        public IEnumerable<JobStatusDTO> FindAll(string nodeName)
        {
            return _statusCache.Where(x => x.NodeName == nodeName);
        }

        public void Persist(IEnumerable<JobStatusDTO> statuses)
        {
            statuses.Each(x => {
                _statusCache[x.Id] = x;
            });
        }

        public void Persist(JobStatusDTO status)
        {
            if (_statusCache.Has(status.Id) && status.LastExecution == null)
            {
                status.LastExecution = _statusCache[status.Id].LastExecution;
            }

            _statusCache[status.Id] = status;
        }

        public JobStatusDTO Find(string nodeName, string jobKey)
        {
            return _statusCache[new JobStatusDTO {NodeName = nodeName, JobKey = jobKey}.Id];
        }

        public JobStatusDTO Load(string nodeName, string jobKey)
        {
            var key = new JobStatusDTO {NodeName = nodeName, JobKey = jobKey}.Id;
            return _statusCache[key];
        }
    }
}