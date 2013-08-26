using System;
using System.Collections.Generic;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.Runtime.Delayed
{
    /* TODO:
     * 1.) Add prop to TransportSettings for the polling time
     * 2.) InMemory
     * 3.) Envelope.ExecutionTime : DateTime?
     * 4.) MessageInvoker.Invoke, if execution-time header is set and not ready, call IMessageCallback.MoveToDelayedQueue.  Log the delayed storage
     *     NEED to deal w/ default content types?
     * 5.) UT DelayedEnvelopeProcessor
     * 6.) Register the polling job against the TransportSettings property
     * 
     */

    public interface IDelayedChannel
    {
        Uri Address { get; }
        void DequeueExpired(DateTime currentTime, IReceiver receiver);
    }

    public class DelayedEnvelopeProcessor : IJob, IReceiver
    {
        private readonly IEnvelopeSender _sender;
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;
        private readonly IEnumerable<ITransport> _transports;

        public DelayedEnvelopeProcessor(IEnvelopeSender sender, ILogger logger, ISystemTime systemTime, IEnumerable<ITransport> transports)
        {
            _sender = sender;
            _logger = logger;
            _systemTime = systemTime;
            _transports = transports;
        }

        public void Execute()
        {
            var currentTime = _systemTime.UtcNow();
            _transports.Each(transport => transport.DelayedChannel().DequeueExpired(currentTime, this));
        }

        void IReceiver.Receive(Envelope envelope, IMessageCallback callback)
        {
            _sender.Send(envelope);
            _logger.InfoMessage(() => new DelayedEnvelopeSent { Envelope = envelope });
        }
    }

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

    public class DelayedEnvelopeSent : LogRecord
    {
        public Envelope Envelope { get; set; }

        protected bool Equals(DelayedEnvelopeSent other)
        {
            return Equals(Envelope, other.Envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayedEnvelopeSent) obj);
        }

        public override int GetHashCode()
        {
            return (Envelope != null ? Envelope.GetHashCode() : 0);
        }
    }
}