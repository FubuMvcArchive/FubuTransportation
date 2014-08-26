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

        public FubuTransportationActivator(TransportActivator transports, SubscriptionActivator subscriptions, IScheduledJobController scheduledJobs)
        {
            _transports = transports;
            _subscriptions = subscriptions;
            _scheduledJobs = scheduledJobs;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _transports.Activate(packages, log);
            _subscriptions.Activate(packages, log);
            _scheduledJobs.Activate();

        }

        public void Deactivate(IPackageLog log)
        {
            log.Trace("Shutting down the scheduled jobs");
            _scheduledJobs.Deactivate();
        }
    }
}