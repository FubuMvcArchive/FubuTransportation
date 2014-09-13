using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.ScheduledJobs.Persistence
{
    public interface ISchedulePersistence
    {
        IEnumerable<JobStatusDTO> FindAll(string nodeName);
        IEnumerable<JobStatusDTO> FindAllActive(string nodeName);
        void Persist(IEnumerable<JobStatusDTO> statuses);
        void Persist(JobStatusDTO status);

        JobStatusDTO Find(string nodeName, string jobKey);

        void RecordHistory(string nodeName, string jobKey, JobExecutionRecord record);
        IEnumerable<JobExecutionRecord> FindHistory(string nodeName, string jobKey);
    }

    public class ScheduledRunHistory
    {
        public string Id { get; set; }
        private readonly Queue<JobExecutionRecord> _records = new Queue<JobExecutionRecord>();

        public JobExecutionRecord[] Records
        {
            get
            {
                return _records.ToArray();
            }
            set
            {
                _records.Clear();
                if (value != null)
                {
                    value.Each(x => _records.Enqueue(x));
                }
            }
        }

        public void Append(JobExecutionRecord record, int maxRecords)
        {
            _records.Enqueue(record);
            while (_records.Count > maxRecords)
            {
                _records.Dequeue();
            }
        }


    }
}