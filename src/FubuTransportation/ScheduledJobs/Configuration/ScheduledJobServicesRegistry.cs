using FubuMVC.Core.Registration;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.ScheduledJobs.Persistence;

namespace FubuTransportation.ScheduledJobs.Configuration
{
    public class ScheduledJobServicesRegistry : ServiceRegistry
    {
        public ScheduledJobServicesRegistry()
        {
            SetServiceIfNone<IScheduledJobController, ScheduledJobController>(x => x.AsSingleton());
            SetServiceIfNone<IJobTimer, JobTimer>(x => x.AsSingleton());

            SetServiceIfNone<ISchedulePersistence, InMemorySchedulePersistence>();

            SetServiceIfNone<IScheduleStatusMonitor, ScheduleStatusMonitor>();
        }
    }
}