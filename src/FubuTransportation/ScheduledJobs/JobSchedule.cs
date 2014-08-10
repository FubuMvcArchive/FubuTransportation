using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;

namespace FubuTransportation.ScheduledJobs
{
    public class JobSchedule : IEnumerable<JobStatus>
    {
        private readonly IList<JobStatus> _changes = new List<JobStatus>();
        private readonly IList<JobStatus> _removals = new List<JobStatus>();

        private readonly Cache<Type, JobStatus> _status =
            new Cache<Type, JobStatus>(x => new JobStatus(x));


        public JobSchedule()
        {
        }

        public JobSchedule(IEnumerable<JobStatus> all)
        {
            all.Each(x => _status[x.JobType] = x);
        }

        public JobStatus Find(Type jobType)
        {
            return _status[jobType];
        }

        public JobStatus Schedule(Type jobType, DateTimeOffset nextTime)
        {
            var status = _status[jobType];
            status.NextTime = nextTime;
            _changes.Fill(status);

            return status;
        }

        public void RemoveObsoleteJobs(IEnumerable<Type> jobTypes)
        {
            var obsoletes = _status.Where(x => !jobTypes.Contains(x.JobType)).ToArray();
            _removals.AddRange(obsoletes);

            obsoletes.Each(x => _status.Remove(x.JobType));

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