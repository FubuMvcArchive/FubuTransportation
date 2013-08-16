using System;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using System.Linq;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;
using FubuCore;
using System.Collections.Generic;

namespace FubuTransportation.Sagas
{
    public interface IStatefulSaga<TState>
    {
        bool IsCompleted();
        TState State { get; set; }
    }

    // I know I didn't like the sound of the double generics before,
    // but I think it makes sense.  This lets us put the responsibility 
    // for resolving the TMessage from IFubuRequest into the SagaBehavior
    // instead of each SagaRepository
    // Also think you need the TState in the signature for Raven to determine how
    // it's going to resolve the TState by it's Id
    public interface ISagaRepository<TState, TMessage>
    {
        void Save(TState state);
        TState Find(TMessage message);
        void Delete(TState state);
    }

    public class StatefulSagaNode : BehaviorNode
    {
        private readonly Type _handlerType;
        private readonly Type _stateType;
        private readonly Type _messageType;

        public StatefulSagaNode(Type handlerType, Type stateType, Type messageType)
        {
            _handlerType = handlerType;
            _stateType = stateType;
            _messageType = messageType;
        }

        public ISagaRepositoryNode Repository { get; set; }

        protected override ObjectDef buildObjectDef()
        {
            if (Repository == null)
            {
                throw new InvalidOperationException("something descriptive here saying you don't know how to do the repo for the saga");
            }

            var def = new ObjectDef(typeof (SagaBehavior<,,>), _stateType, _messageType, _handlerType);
            var repositoryType = typeof (ISagaRepository<,>).MakeGenericType(_stateType, _messageType);
            def.Dependency(repositoryType, Repository.BuildRepositoryDef(_stateType, _messageType));

            return def;
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Wrapper; }
        }

        public Type StateType
        {
            get { return _stateType; }
        }

        public Type MessageType
        {
            get { return _messageType; }
        }
    }

    // Thinking that there would be one for RavenDb sagas etc.
    // Smart enough to do the expression/func business to get at correlation id's
    public interface ISagaRepositoryNode
    {
        ObjectDef BuildRepositoryDef(Type stateType, Type messageType);
    }

    // Thinking that something else is going to have to come from behind for the repo's
    public class StatefulSagaConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var sagaHandlers = graph.Behaviors.SelectMany(x => x).OfType<HandlerCall>()
                 .Where(x => x.HandlerType.Closes(typeof (IStatefulSaga<>)))
                 .ToArray();

            sagaHandlers.Each(call => {
                var sagaInterface = call.HandlerType.FindInterfaceThatCloses(typeof (IStatefulSaga<>));
                var stateType = sagaInterface.GetGenericArguments().Single();
                var messageType = call.InputType();

                var sagaNode = new StatefulSagaNode(call.HandlerType, stateType, messageType);
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
    }
}