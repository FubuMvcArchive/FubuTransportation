using FubuCore.Logging;

namespace FubuTransportation.Runtime.Delayed
{
    public class DelayedEnvelopeAddedBackToQueue : LogRecord
    {
        public EnvelopeToken Envelope { get; set; }

        protected bool Equals(DelayedEnvelopeAddedBackToQueue other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayedEnvelopeAddedBackToQueue) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }
    }
}