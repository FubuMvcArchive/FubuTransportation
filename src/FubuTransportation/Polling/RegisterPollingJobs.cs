using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Polling
{
    [ConfigurationType(ConfigurationType.Services)]
    public class RegisterPollingJobs : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var settings = graph.Settings.Get<PollingJobSettings>();
            settings.Jobs.Select(x => x.ToObjectDef()).Each(x => {
                graph.Services.AddService(typeof(IPollingJob), x);
            });
        }
    }
}