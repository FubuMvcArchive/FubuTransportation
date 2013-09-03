using FubuCore.Logging;
using FubuTransportation.Logging;
using FubuTransportation.Runtime.Cascading;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainSuccessContinuation : IContinuation
    {
        private readonly IOutgoingSender _sender;
        private readonly IInvocationContext _context;

        public ChainSuccessContinuation(IOutgoingSender sender, IInvocationContext context)
        {
            _sender = sender;
            _context = context;
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            _sender.SendOutgoingMessages(envelope, _context.OutgoingMessages());

            envelope.Callback.MarkSuccessful();
            context.Logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope.ToToken() });
        }

        public IInvocationContext Context
        {
            get { return _context; }
        }
    }
}