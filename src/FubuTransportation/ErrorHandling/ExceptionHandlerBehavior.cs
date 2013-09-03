using System;
using System.Collections.Generic;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class ExceptionHandlerBehavior : IActionBehavior
    {
        private readonly IActionBehavior _behavior;
        private readonly HandlerChain _chain;
        private readonly Envelope _envelope;
        private readonly IInvocationContext _context;
        private readonly ILogger _logger;

        public ExceptionHandlerBehavior(IActionBehavior behavior, HandlerChain chain, Envelope envelope, IInvocationContext context, ILogger logger)
        {
            _behavior = behavior;
            _chain = chain;
            _envelope = envelope;
            _context = context;
            _logger = logger;
        }

        public void Invoke()
        {
            try
            {
                _behavior.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(_envelope.CorrelationId, ex);
                _context.Continuation = DetermineContinuation(ex);
            }
        }

        public IContinuation DetermineContinuation(Exception ex)
        {
            if (_envelope.Attempts >= _chain.MaximumAttempts)
            {
                return new MoveToErrorQueue(ex);
            }

            return _chain.ErrorHandlers.FirstValue(x => x.DetermineContinuation(_envelope, ex))
                   ?? new MoveToErrorQueue(ex);
        }

        public HandlerChain Chain
        {
            get { return _chain; }
        }

        public void InvokePartial()
        {
            _behavior.InvokePartial();
        }
    }
}