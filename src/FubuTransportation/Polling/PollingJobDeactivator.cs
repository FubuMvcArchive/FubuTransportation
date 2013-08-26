using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;

namespace FubuTransportation.Polling
{
    public class PollingJobDeactivator : IDeactivator
    {
        private readonly IPollingJobs _jobs;

        public PollingJobDeactivator(IPollingJobs jobs)
        {
            _jobs = jobs;
        }

        public void Deactivate(IPackageLog log)
        {
            _jobs.Each(x => {
                try
                {
                    x.Stop();
                }
                catch (Exception ex)
                {
                    log.MarkFailure(ex);
                }
            });
        }
    }
}