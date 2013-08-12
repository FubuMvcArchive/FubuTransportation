using System;
using System.Collections;
using System.Collections.Generic;
using FubuCore.Binding;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Runtime
{
    public class HandlerArguments : ServiceArguments, IOutgoingMessages
    {
        private readonly Envelope _envelope;
        private readonly IList<object> _messages = new List<object>();

        public HandlerArguments(Envelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException("envelope");
            
            _envelope = envelope;
            var inputType = envelope.Message.GetType();
            var request = new InMemoryFubuRequest();
            request.Set(inputType, _envelope.Message);
            
            Set(typeof(IFubuRequest), request);
            Set(typeof(IOutgoingMessages), this);
            Set(typeof(Envelope), envelope);
        }

        public Envelope Envelope
        {
            get { return _envelope; }
        }

        public void Enqueue(object message)
        {
            var enumerable = message as IEnumerable<object>;
            if (enumerable == null)
            {
                _messages.Add(message);
            }
            else
            {
                _messages.AddRange(enumerable);
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected bool Equals(HandlerArguments other)
        {
            return Equals(_envelope, other._envelope);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HandlerArguments) obj);
        }

        public override int GetHashCode()
        {
            return (_envelope != null ? _envelope.GetHashCode() : 0);
        }
    }
}