using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobServicesRegistry : ServiceRegistry
    {
        public ScheduledJobServicesRegistry()
        {
            SetServiceIfNone<IScheduledJobLogger, ScheduledJobLogger>();
        }
    }
}