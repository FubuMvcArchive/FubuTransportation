using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public class NoSubscriberHandler : SimpleEnvelopeHandler
    {
        public override bool Matches(Envelope envelope)
        {
            return true;
        }

        public override void Execute(Envelope envelope, ContinuationContext context)
        {
            envelope.Callback.MarkSuccessful();
            // TODO -- do more here.  There's a GH issue for this.
        }
    }
}