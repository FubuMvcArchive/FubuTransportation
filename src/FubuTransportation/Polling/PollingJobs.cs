using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Polling
{
    public class PollingJobs : IPollingJobs
    {
        private readonly IEnumerable<IPollingJob> _jobs;

        public PollingJobs(IEnumerable<IPollingJob> jobs)
        {
            _jobs = jobs;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPollingJob> GetEnumerator()
        {
            return _jobs.GetEnumerator();
        }

        public bool IsActive<T>() where T : IJob
        {
            return IsActive(typeof (T));
        }

        public IPollingJob For(Type jobType)
        {
            return _jobs.FirstOrDefault(x => x.JobType == jobType);
        }

        public bool IsActive(Type jobType)
        {
            var job = For(jobType);
            return job == null ? false : job.IsRunning();
        }

        public void Activate<T>() where T : IJob
        {
            Activate(typeof(T));
        }

        public void Activate(Type type)
        {
            var job = For(type);
            if (job != null) job.Start();
        }
    }
}