using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Ajax;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Publishing
{
    [ConfigurationType(ConfigurationType.Explicit)]
    public class EventPublisherConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var transformers = graph.FirstActions().Where(x => x.HandlerType.CanBeCastTo<IEventPublisher>());
            transformers.Each(x => {
                x.AddAfter(new PublishEvent(x));
                var chain = x.ParentChain();
                chain.ResourceType(typeof (AjaxContinuation));
            });
        }
    }
}