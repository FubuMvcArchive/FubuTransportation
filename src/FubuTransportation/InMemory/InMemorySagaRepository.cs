using System;
using FubuCore.Util;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Runtime
{
    //TODO Should we serialize the state?
    public class InMemorySagaRepository<TMessage> : ISagaRepository<TMessage> where TMessage : class
    {
        private readonly Lazy<Guid> _correlationId; 
        private static Cache<Guid, object> _states = new Cache<Guid, object>(); 

        public InMemorySagaRepository(IFubuRequest fubuRequest, Func<TMessage, Guid> correlationIdGet)
        {
            _correlationId = new Lazy<Guid>(() => correlationIdGet(fubuRequest.Get<TMessage>()));
        }

        public void Save<TState>(TState state) where TState : class
        {
            _states[_correlationId.Value] = state;
        }

        public TState Load<TState>() where TState : class
        {
            if (!_states.Has(_correlationId.Value))
                return null;
            return _states[_correlationId.Value] as TState;
        }

        public void Delete<TState>(TState state) where TState : class
        {
            _states.Remove(_correlationId.Value);
        }
    }
}