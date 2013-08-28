using FubuCore.Logging;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class ChainSuccessContinuation : IContinuation
    {
        private readonly IEnvelopeSender _sender;
        private readonly IInvocationContext _context;

        public ChainSuccessContinuation(IEnvelopeSender sender, IInvocationContext context)
        {
            _sender = sender;
            _context = context;
        }

        public void Execute(Envelope envelope, ILogger logger)
        {
            _sender.SendOutgoingMessages(envelope, _context.OutgoingMessages());

            envelope.Callback.MarkSuccessful();
            logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope.ToToken() });
        }

        public IInvocationContext Context
        {
            get { return _context; }
        }
    }
}