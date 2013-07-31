using System;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public class Receiver : IReceiver
    {
        private readonly IMessageInvoker _messageInvoker;
        private readonly ChannelGraph _graph;
        private readonly ChannelNode _node;
        private readonly Uri _address;

        // TODO -- take in ChannelNode.  
        public Receiver(IMessageInvoker messageInvoker, ChannelGraph graph, ChannelNode node)
        {
            _messageInvoker = messageInvoker;
            _graph = graph;
            _node = node;
            _address = node.Uri;
        }

        // TODO -- remove IChannel from this signature
        public void Receive(Envelope envelope)
        {
            envelope.Source = _address;
            envelope.ContentType = envelope.ContentType ?? _node.DefaultContentType ?? _graph.DefaultContentType;

            _messageInvoker.Invoke(envelope);
        }
    }
}