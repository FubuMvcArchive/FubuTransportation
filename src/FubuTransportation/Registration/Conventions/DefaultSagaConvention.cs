using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;

namespace FubuTransportation.Registration.Conventions
{
    [ConfigurationType(ConfigurationType.Attachment)] 
    public class DefaultSagaConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.Handlers()
                .Where(x => x.HandlerType.MatchesSagaConvention())
                .Each(x =>
                {
                    var property = x.HandlerType.GetProperty("State");
                    var sagaNode = new SagaNode(x.HandlerType, property.PropertyType, x.InputType(),
                        x.Method.Name.StartsWith("Initiates"));
                    var defaultRepositoryDef = graph.Services.DefaultServiceFor(typeof(ISagaRepository<>));
                    sagaNode.SagaRepositoryByCorrelationId(defaultRepositoryDef);
                    x.AddBefore(sagaNode);
                });
        }
    }
}