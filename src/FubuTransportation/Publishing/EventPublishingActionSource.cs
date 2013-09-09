using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;

namespace FubuTransportation.Publishing
{
    [ConfigurationType(ConfigurationType.Discovery)]
    public class EventPublishingActionSource : IActionSource
    {
        public void Configure(BehaviorGraph graph)
        {
            var pool = TypePool.AppDomainTypes();
            pool.TypesMatching(type => type.IsConcreteTypeOf<IEventPublisher>()).Each(type => {
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(ActionSource.IsCandidate)
                    .Each(method => {
                        var transform = new ActionCall(type, method);
                        var chain = new BehaviorChain();
                        chain.AddToEnd(transform);

                        graph.AddChain(chain);
                    });
            });
        }

        public IEnumerable<ActionCall> FindActions(Assembly applicationAssembly)
        {
            var pool = TypePool.AppDomainTypes();
            var types = pool.TypesMatching(type => type.IsConcreteTypeOf<IEventPublisher>());
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(ActionSource.IsCandidate);

                foreach (var method in methods)
                {
                    yield return new ActionCall(type, method);
                }
            }
        }
    }
}