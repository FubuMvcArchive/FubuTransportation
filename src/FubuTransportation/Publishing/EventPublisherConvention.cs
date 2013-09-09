using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Ajax;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Publishing
{
    [ConfigurationType(ConfigurationType.Policy)]
    public class EventPublisherConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var transformers = graph.FirstActions().Where(x => FubuCore.TypeExtensions.CanBeCastTo<IEventPublisher>(x.HandlerType));
            transformers.Each(x => {
                x.AddAfter(new PublishEvent(x));
                var chain = x.ParentChain();
                chain.ResourceType(typeof (AjaxContinuation));
            });
        }
    }
}