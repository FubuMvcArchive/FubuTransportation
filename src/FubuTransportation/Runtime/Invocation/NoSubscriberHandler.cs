using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class NoSubscriberHandler : SimpleEnvelopeHandler
    {
        public override bool Matches(Envelope envelope)
        {
            return true;
        }

        public override void Execute(Envelope envelope, ILogger logger)
        {
            envelope.Callback.MarkSuccessful();
            // TODO -- do more here.  Post haste
        }
    }
}