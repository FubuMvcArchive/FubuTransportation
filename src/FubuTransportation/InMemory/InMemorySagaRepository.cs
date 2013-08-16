using System;
using System.Collections.Generic;
using System.Threading;
using FubuMVC.Core.Runtime;
using FubuTransportation.Sagas;

namespace FubuTransportation.InMemory
{
    public class InMemorySagaRepository<TState, TMessage> : ISagaRepository<TState, TMessage> where TState : class
    {
        private readonly Func<TMessage, Guid> _messageGetter;
        private readonly Func<TState, Guid> _stateGetter;
        private readonly ISagaStateCache<TState> _cache;

        public static InMemorySagaRepository<TState, TMessage> Create()
        {
            var types = new SagaTypes
            {
                StateType = typeof (TState),
                MessageType = typeof (TMessage)
            };

            return new InMemorySagaRepository<TState, TMessage>((Func<TMessage, Guid>) types.ToCorrelationIdFunc(), (Func<TState, Guid>) types.ToSagaIdFunc(), new SagaStateCache<TState>());
        } 

        public InMemorySagaRepository(Func<TMessage, Guid> messageGetter, Func<TState, Guid> stateGetter, ISagaStateCache<TState> cache)
        {
            _messageGetter = messageGetter;
            _stateGetter = stateGetter;
            _cache = cache;
        }

        public void Save(TState state)
        {
            _cache.Store(_stateGetter(state), state);
        }

        public TState Find(TMessage message)
        {
            var correlationId = _messageGetter(message);
            return _cache.Find(correlationId);
        }

        public void Delete(TState state)
        {
            var id = _stateGetter(state);
            _cache.Delete(id);
        }
    }
}