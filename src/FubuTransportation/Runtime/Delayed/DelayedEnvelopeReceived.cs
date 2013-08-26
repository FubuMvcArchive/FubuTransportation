using FubuCore.Logging;

namespace FubuTransportation.Runtime.Delayed
{
    public class DelayedEnvelopeReceived : LogRecord
    {
        public Envelope Envelope { get; set; }

        protected bool Equals(DelayedEnvelopeReceived other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayedEnvelopeReceived) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }
    }
}