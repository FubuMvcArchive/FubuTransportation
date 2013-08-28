using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class ResponseEnvelopeHandler : SimpleEnvelopeHandler
    {
        public override bool Matches(Envelope envelope)
        {
            return envelope.ResponseId.IsNotEmpty();
        }

        public override void Execute(Envelope envelope, ILogger logger)
        {
            logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope.ToToken() });
            envelope.Callback.MarkSuccessful();
        }
    }
}