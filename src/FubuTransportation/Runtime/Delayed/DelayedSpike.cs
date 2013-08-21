using System;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.Runtime.Delayed
{
    /* TODO:
     * 1.) Add prop to TransportSettings for the polling time
     * 2.) InMemoryDelayedEnvelopeStorage
     * 3.) Envelope.ExecutionTime : DateTime?
     * 4.) MessageInvoker.Invoke, if execution-time header is set and not ready, put into IDelayedEnvelopeStorage.  Log the delayed storage
     * 5.) UT DelayedEnvelopeProcessor
     * 6.) Register the polling job against the TransportSettings property
     * 
     */


    public interface IDelayedEnvelopeStorage
    {
        void Store(DateTime requestedTime, Envelope envelope);
        void Dequeue(DateTime now, Action<Envelope> callback);

        // TODO -- do something for diagnostics later
    }

    public class DelayedEnvelopeProcessor : IJob
    {
        private readonly IEnvelopeSender _sender;
        private readonly IDelayedEnvelopeStorage _storage;
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;

        public DelayedEnvelopeProcessor(IEnvelopeSender sender, IDelayedEnvelopeStorage storage, ILogger logger, ISystemTime systemTime)
        {
            _sender = sender;
            _storage = storage;
            _logger = logger;
            _systemTime = systemTime;
        }

        public void Execute()
        {
            _storage.Dequeue(_systemTime.UtcNow(), env => {
                _sender.Send(env);
                _logger.InfoMessage(() => new DelayedEnvelopeSent{Envelope = env});
            });
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