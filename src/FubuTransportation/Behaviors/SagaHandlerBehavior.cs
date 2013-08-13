using System;
using FubuMVC.Core.Behaviors;
using FubuTransportation.Runtime;

namespace FubuTransportation.Behaviors
{
    public class SagaHandlerBehavior<THandler, TSagaState, TMessage> : IActionBehavior
        where THandler : class
        where TMessage : class
        where TSagaState : class, new()
    {
        private readonly THandler _handler;
        private readonly ISagaRepository<TMessage> _sagaRepository;
        private readonly Action<THandler, TSagaState> _stateSetter;
        private readonly Func<THandler, bool> _isComplete;

        public SagaHandlerBehavior(THandler handler, 
            ISagaRepository<TMessage> sagaRepository, 
            Action<THandler, TSagaState> stateSetter, 
            Func<THandler, bool> isComplete)
        {
            _handler = handler;
            _sagaRepository = sagaRepository;
            _stateSetter = stateSetter;
            _isComplete = isComplete;
        }

        public IActionBehavior Inner { get; set; }

        public void Invoke()
        {
            var state = LoadState();
            if (state == null)
                return; //TODO put this message in a holding queue, timed message?

            _stateSetter(_handler, state);
            Inner.Invoke();

            if (_isComplete(_handler))
            {
                _sagaRepository.Delete(state);
            }
            else
            {
                _sagaRepository.Save(state);
            }
        }

        protected virtual TSagaState LoadState()
        {
            var state = _sagaRepository.Load<TSagaState>();
            return state;
        }

        public void InvokePartial()
        {
            Invoke();
        }
    }
}