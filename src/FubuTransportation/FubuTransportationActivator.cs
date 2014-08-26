using System.Collections.Generic;
using System.Threading.Tasks;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Runtime;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.Subscriptions;

namespace FubuTransportation
{
    public class FubuTransportationActivator : IActivator, IDeactivator
    {
        private readonly TransportActivator _transports;
        private readonly SubscriptionActivator _subscriptions;
        private readonly IScheduledJobController _scheduledJobs;
        private readonly ScheduledJobGraph _scheduledJobGraph;

        public FubuTransportationActivator(TransportActivator transports, SubscriptionActivator subscriptions, IScheduledJobController scheduledJobs, ScheduledJobGraph scheduledJobGraph)
        {
            _transports = transports;
            _subscriptions = subscriptions;
            _scheduledJobs = scheduledJobs;
            _scheduledJobGraph = scheduledJobGraph;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _transports.Activate(packages, log);
            _subscriptions.Activate(packages, log);

            if (_scheduledJobGraph.ActivateOnStartup)
            {
                _scheduledJobs.Activate();
            }
            

        }

        public void Deactivate(IPackageLog log)
        {
            log.Trace("Shutting down the scheduled jobs");
            _scheduledJobs.Deactivate();
        }
    }
}