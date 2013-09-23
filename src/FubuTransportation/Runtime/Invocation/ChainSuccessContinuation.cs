using System.ComponentModel;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    [Description("The handler chain was successful, dequeues the envelope")]
    public class ChainSuccessContinuation : IContinuation
    {
        private readonly IInvocationContext _context;

        public ChainSuccessContinuation(IInvocationContext context)
        {
            _context = context;
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            context.Outgoing.SendOutgoingMessages(envelope, _context.OutgoingMessages());

            envelope.Callback.MarkSuccessful();
            context.Logger.InfoMessage(() => new MessageSuccessful {Envelope = envelope.ToToken()});
        }

        public IInvocationContext Context
        {
            get { return _context; }
        }
    }
}