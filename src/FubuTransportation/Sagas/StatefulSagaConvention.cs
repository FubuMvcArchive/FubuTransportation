using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.Sagas
{
    public class StatefulSagaConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var sagaHandlers = graph.Behaviors.SelectMany(x => x).OfType<HandlerCall>()
                                    .Where(IsSagaHandler)
                                    .ToArray();

            sagaHandlers.Each(call => {
                var types = ToSagaTypes(call);

                var sagaNode = new StatefulSagaNode(types);
                call.AddBefore(sagaNode);
            });
        }

        public static bool IsSagaHandler(HandlerCall call)
        {
            return call.HandlerType.Closes(typeof (IStatefulSaga<>));
        }

        public static bool IsSagaChain(BehaviorChain chain)
        {
            if (chain is HandlerChain)
            {
                return chain.OfType<HandlerCall>().Any(IsSagaHandler);
            }

            return false;
        }

        public static SagaTypes ToSagaTypes(HandlerCall call)
        {
            return new SagaTypes
            {
                HandlerType = call.HandlerType,
                MessageType = call.InputType(),
                StateType = call.HandlerType.FindInterfaceThatCloses(typeof(IStatefulSaga<>)).GetGenericArguments().Single()
            };
        }
    }
}