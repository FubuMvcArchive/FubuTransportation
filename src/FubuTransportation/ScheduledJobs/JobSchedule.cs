using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;

namespace FubuTransportation.ScheduledJobs
{
    public class JobSchedule : IEnumerable<JobStatus>
    {
        private readonly IList<JobStatus> _changes = new List<JobStatus>();
        private readonly IList<JobStatus> _removals = new List<JobStatus>();

        private readonly Cache<string, JobStatus> _status =
            new Cache<string, JobStatus>(x => new JobStatus {JobType = x});


        public JobSchedule()
        {
        }

        public JobSchedule(IEnumerable<JobStatus> all)
        {
            all.Each(x => _status[x.JobType] = x);
        }

        public IJobStatus Find(Type jobType)
        {
            return _status[jobType.FullName];
        }

        public IJobStatus Schedule(Type jobType, DateTimeOffset nextTime)
        {
            var status = _status[jobType.FullName];
            status.NextTime = nextTime;
            _changes.Fill(status);

            return status;
        }

        public void RemoveObsoleteJobs(IEnumerable<Type> jobTypes)
        {
            var names = jobTypes.Select(x => x.FullName).ToArray();

            var obsoletes = _status.Where(x => !names.Contains(x.JobType));
            _removals.AddRange(obsoletes);
        }

        public IEnumerable<JobStatus> Changes()
        {
            return _changes;
        }

        public IEnumerable<JobStatus> Removals()
        {
            return _removals;
        }

        public IEnumerator<JobStatus> GetEnumerator()
        {
            return _status.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}