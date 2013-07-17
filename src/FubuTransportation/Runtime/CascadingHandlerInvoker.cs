using System;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Runtime
{
    public class CascadingHandlerInvoker<THandler, TInput, TOutput> : BasicBehavior where TInput : class
    {
        private readonly IFubuRequest _request;
        private readonly THandler _handler;
        private readonly IOutgoingMessages _messages;
        private readonly Func<THandler, TInput, TOutput> _action;

        public CascadingHandlerInvoker(IFubuRequest request, THandler handler, Func<THandler, TInput, TOutput> action, IOutgoingMessages messages) : base(PartialBehavior.Executes)
        {
            _request = request;
            _handler = handler;
            _action = action;
            _messages = messages;
        }

        protected override DoNext performInvoke()
        {
            var input = _request.Find<TInput>().Single();
            var output = _action(_handler, input);

            _messages.Enqueue(output);

            return DoNext.Continue;
        }
    }
}