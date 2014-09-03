using System;
using System.Collections.Generic;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuCore.Descriptions;

namespace FubuTransportation.Polling
{
    public class PollingJobActivator : IActivator
    {
        private readonly IPollingJobs _jobs;

        public PollingJobActivator(IPollingJobs jobs)
        {
            _jobs = jobs;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _jobs.Where(x => x.ScheduledExecution != ScheduledExecution.Disabled).Each(x => {
                try
                {
                    log.Trace("Starting " + Description.For(x).Title);
                    x.Start();
                }
                catch (Exception ex)
                {
                    log.MarkFailure(ex);
                }
            });
        }
    }
}