using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobServicesRegistry : ServiceRegistry
    {
        public ScheduledJobServicesRegistry()
        {
            SetServiceIfNone<IScheduledJobController, ScheduledJobController>(x => x.AsSingleton());
            SetServiceIfNone<IJobTimer, JobTimer>(x => x.AsSingleton());

            SetServiceIfNone<ISchedulePersistence, InMemorySchedulePersistence>();

            SetServiceIfNone<IScheduleRepository, ScheduleRepository>();
        }
    }
}