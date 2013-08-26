using System.Collections;
using System.Collections.Generic;

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
    }
}