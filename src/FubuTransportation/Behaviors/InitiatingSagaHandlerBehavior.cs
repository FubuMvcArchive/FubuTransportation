using System;
using FubuTransportation.Runtime;

namespace FubuTransportation.Behaviors
{
    public class InitiatingSagaHandlerBehavior<THandler, TSagaState, TMessage> : SagaHandlerBehavior<THandler, TSagaState, TMessage> 
        where THandler : class
        where TMessage : class
        where TSagaState : class, new()
    {
        public InitiatingSagaHandlerBehavior(THandler handler, 
            ISagaRepository<TMessage> sagaRepository, 
            Action<THandler, TSagaState> stateSetter, 
            Func<THandler, bool> isComplete)
            : base(handler, sagaRepository, stateSetter, isComplete)
        {
        }

        protected override TSagaState LoadState()
        {
            return base.LoadState() ?? new TSagaState();
        }
    }
}