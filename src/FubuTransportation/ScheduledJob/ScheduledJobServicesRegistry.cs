using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobServicesRegistry : ServiceRegistry
    {
        public ScheduledJobServicesRegistry()
        {
            SetServiceIfNone<IScheduledJobLogger, ScheduledJobLogger>();
        }
    }
}