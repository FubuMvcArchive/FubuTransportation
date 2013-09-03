using System;
using System.Threading.Tasks;
using FubuTransportation.Events;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime
{
    public class ReplyListener<T> : IListener<EnvelopeReceived>, IExpiringListener
    {
        private readonly IEventAggregator _events;
        private readonly TaskCompletionSource<T> _completion;
        private readonly string _originalId;
        
        // TODO -- do an expiration on this thing.
        public ReplyListener(IEventAggregator events, string originalId, TimeSpan timeout)
        {
            _events = events;
            _completion = new TaskCompletionSource<T>();
            ExpiresAt = DateTime.UtcNow.Add(timeout);
            _originalId = originalId;
        }

        public Task<T> Task
        {
            get { return _completion.Task; }
        }

        public void Handle(EnvelopeReceived message)
        {
            if (Matches(message.Envelope))
            {
                _completion.SetResult((T) message.Envelope.Message);
                _events.RemoveListener(this);

                IsExpired = true;
            }
        }

        public bool Matches(EnvelopeToken envelope)
        {
            return envelope.Message is T && envelope.ResponseId == _originalId;
        }

        protected bool Equals(ReplyListener<T> other)
        {
            return string.Equals(_originalId, other._originalId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReplyListener<T>) obj);
        }

        public override int GetHashCode()
        {
            return (_originalId != null ? _originalId.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Reply watcher for {0} with Id {1}",typeof(T).FullName, _originalId);
        }

        public bool IsExpired { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
    }
}