using FubuMVC.Core;
using FubuMVC.Core.Registration;
using System.Collections.Generic;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuTransportation.InMemory
{
    [ConfigurationType(ConfigurationType.Policy)]
    public class AllQueuesInMemoryPolicy : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var settings = graph.Settings.Get<TransportSettings>();
            settings.SettingTypes.Each(settingType => {
                var settingObject = InMemoryTransport.ToInMemory(settingType);

                graph.Services.AddService(settingType, ObjectDef.ForValue(settingObject));
            });
        }
    }
}