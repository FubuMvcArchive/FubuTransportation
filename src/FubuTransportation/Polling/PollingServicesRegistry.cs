using Bottles;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Polling
{
    public class PollingServicesRegistry : ServiceRegistry
    {
        public PollingServicesRegistry()
        {
            // NEED MORE.
            SetServiceIfNone<ITimer, DefaultTimer>();
            AddService<IActivator, PollingJobActivator>();
            AddService<IDeactivator, PollingJobDeactivator>();
            SetServiceIfNone<IPollingJobs, PollingJobs>();
        }
    }
}