using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;

namespace FubuTransportation.Publishing
{
    /// <summary>
    /// Strictly a marker interface to denote that this class
    /// should be exposed 
    /// published to the service bus the results of its methods
    /// </summary>
    public interface IEventPublisher
    {
        
    }



    [ConfigurationType(ConfigurationType.Discovery)]
    public class EventPublishing : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var pool = TypePool.AppDomainTypes();
            pool.TypesMatching(type => type.IsConcreteTypeOf<IEventPublisher>()).Each(type => {
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(ActionSource.IsCandidate)
                    .Each(method => {
                        var transform = new EventTransform(type, method);
                        var chain = new BehaviorChain();
                        chain.AddToEnd(transform);

                        graph.AddChain(chain);
                    });
            });
        }
    }

    public class EventTransform : ActionCallBase, IMayHaveInputType
    {
        public EventTransform(Type handlerType, MethodInfo method) : base(handlerType, method)
        {
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Process; }
        }
    }

    [ConfigurationType(ConfigurationType.Policy)]
    public class EventPublisherConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var transformers = graph.Behaviors.SelectMany(x => x.OfType<EventTransform>()).ToArray();
            transformers.Each(x => x.AddAfter(new PublishEvent(x)));
        }
    }

    public class PublishEvent : Process
    {
        public PublishEvent(EventTransform transform) : base(typeof(EventPublisher<>).MakeGenericType(transform.ResourceType()))
        {
            
        }
    }
}