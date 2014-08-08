using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJob
{
    [ConfigurationType(ConfigurationType.Policy)]
    public class RegisterScheduledJobs : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var settings = graph.Settings.Get<ScheduledJobSettings>();

            settings.Jobs
                    .Select(x => x.ToObjectDef())
                    .Each(x => graph.Services.AddService(typeof(IScheduledJobInitializer), x));
        }
    }
}