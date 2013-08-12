using System;
using FubuCore.Logging;
using FubuTransportation.Configuration;
using System.Collections.Generic;

namespace FubuTransportation.Runtime
{
    public class Receiver : IReceiver
    {
        private readonly IMessageInvoker _messageInvoker;
        private readonly ChannelGraph _graph;
        private readonly ChannelNode _node;
        private readonly IEnvelopeSender _sender;
        private readonly Uri _address;

        public Receiver(IMessageInvoker messageInvoker, ChannelGraph graph, ChannelNode node, IEnvelopeSender sender)
        {
            _messageInvoker = messageInvoker;
            _graph = graph;
            _node = node;
            _sender = sender;
            _address = node.Uri;
        }

        public void Receive(Envelope envelope, IMessageCallback callback)
        {
            envelope.Source = _address;
            envelope.ContentType = envelope.ContentType ?? _node.DefaultContentType ?? _graph.DefaultContentType;

            _messageInvoker.Invoke(envelope, callback);
        }

        protected bool Equals(Receiver other)
        {
            return Equals(_messageInvoker, other._messageInvoker) && Equals(_graph, other._graph) && Equals(_node, other._node);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Receiver) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_messageInvoker != null ? _messageInvoker.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_graph != null ? _graph.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_node != null ? _node.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}