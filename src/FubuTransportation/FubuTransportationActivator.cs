using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuTransportation.Polling;
using FubuTransportation.Runtime;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.Subscriptions;

namespace FubuTransportation
{
    public class FubuTransportationActivator : IActivator, IDeactivator
    {
        private readonly TransportActivator _transports;
        private readonly SubscriptionActivator _subscriptions;
        private readonly IScheduledJobController _scheduledJobs;
        private readonly PollingJobActivator _pollingJobs;

        public FubuTransportationActivator(TransportActivator transports, SubscriptionActivator subscriptions,
            IScheduledJobController scheduledJobs, PollingJobActivator pollingJobs)
        {
            _transports = transports;
            _subscriptions = subscriptions;
            _scheduledJobs = scheduledJobs;
            _pollingJobs = pollingJobs;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            PackageRegistry.Timer.Record("Activating Transports and Starting Listening",
                () => _transports.Activate(packages, log));

            PackageRegistry.Timer.Record("Activating Subscriptions", () => _subscriptions.Activate(packages, log));

            PackageRegistry.Timer.Record("Activating Polling Jobs", () => _pollingJobs.Activate(packages, log));
        }

        public void Deactivate(IPackageLog log)
        {
            log.Trace("Shutting down the scheduled jobs");
            _scheduledJobs.Deactivate();
        }
    }
}