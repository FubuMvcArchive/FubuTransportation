using FubuCore.Logging;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Monitoring
{
    public class MonitoringServiceRegistry : ServiceRegistry
    {
        public MonitoringServiceRegistry()
        {
            AddService<ILogModifier, PersistentTaskMessageModifier>();
        }
    }
}