using Bottles;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;

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


            var def = ObjectDef.ForType<PollingJobs>();
            def.IsSingleton = true;
            SetServiceIfNone(typeof (IPollingJobs), def);
        }
    }
}