using System.Collections;
using System.Linq;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.Async
{
    /* TODO
     * DONE 1.) HandlerCall.IsAsync()
     * DONE 2.) HandlerCall builds AsyncHandlerInvoker appropriately
     * DONE 3.) HandlerCall builds CascadingAsynchHandlerInvoker appropriately
     * DONE 4.) Register IAsyncHandling
     * 5.) AsyncHandlingNode & AsynchHandlingConvention
     * DONE 6.) HandlerChain.IsAsync() : bool
     * 7.) ChainExecutionEnvelopeHandler needs to return the AsyncChainExecutionContinuation
     * 8.) some end to end tests!
     */

    // TODO -- need to register this
    public class AsyncHandlingConvention : HandlerChainPolicy    
    {
        public override bool Matches(HandlerChain chain)
        {
            return chain.IsAsync;
        }

        public override void Configure(HandlerChain handlerChain)
        {
            var firstCall = handlerChain.OfType<HandlerCall>().First();
            firstCall.AddBefore(new AsyncHandlingNode());
        }
    }

    public class AsyncHandlingNode : Wrapper
    {
        public AsyncHandlingNode() : base(typeof(AsyncHandlingBehavior))
        {
        }
    }

    // This will be inserted right before all the asynch calls
    public class AsyncHandlingBehavior : BasicBehavior
    {
        private readonly IAsyncHandling _asyncHandling;

        public AsyncHandlingBehavior(IAsyncHandling asyncHandling)
            : base(PartialBehavior.Executes)
        {
            _asyncHandling = asyncHandling;
        }

        protected override void afterInsideBehavior()
        {
            _asyncHandling.WaitForAll();
        }
    }
}