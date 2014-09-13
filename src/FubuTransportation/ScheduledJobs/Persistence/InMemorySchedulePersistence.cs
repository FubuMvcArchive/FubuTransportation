using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuTransportation.ScheduledJobs.Persistence
{
    public class InMemorySchedulePersistence : ISchedulePersistence
    {
        private readonly Cache<string, JobStatusDTO> _statusCache = new Cache<string, JobStatusDTO>(id => {
            var parts = id.Split('/');
            return new JobStatusDTO {JobKey = parts.Last(), NodeName = parts.First()};
        }); 

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
            return _statusCache[JobStatusDTO.ToId(nodeName, jobKey)];
        }

        public void RecordHistory(string nodeName, string jobKey, JobExecutionRecord record)
        {
            Find(nodeName, jobKey).History.Append(record, 100);
        }

        public IEnumerable<JobExecutionRecord> FindHistory(string nodeName, string jobKey)
        {
            return Find(nodeName, jobKey).History.Records;
        }

    }
}