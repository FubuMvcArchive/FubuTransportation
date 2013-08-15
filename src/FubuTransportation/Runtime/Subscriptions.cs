using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public class Subscriptions : ISubscriptions, IDisposable
    {
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<ITransport> _transports;
        private readonly Lazy<IMessageInvoker> _invoker;

        public Subscriptions(ChannelGraph graph, Func<IMessageInvoker> invoker, IEnumerable<ITransport> transports)
        {
            _graph = graph;
            _transports = transports;
            _invoker = new Lazy<IMessageInvoker>(invoker);
        }

        public IEnumerable<ChannelNode> FindChannels(Envelope envelope)
        {
            if (envelope.Destination != null)
            {
                var destination = findDestination(envelope);

                return new ChannelNode[]{destination};
            }

            // TODO -- gets a LOT more sophisticated later
            var inputType = envelope.Message.GetType();
            return _graph.Where(c => c.Rules.Any(x => x.Matches(inputType)));
        }

        private ChannelNode findDestination(Envelope envelope)
        {
            var destination = _graph.FirstOrDefault(x => x.Uri == envelope.Destination);
            if (destination == null)
            {
                var transport = _transports.FirstOrDefault(x => x.Protocol == envelope.Destination.Scheme);
                if (transport == null)
                {
                    throw new UnknownChannelException(envelope.Destination);
                }

                var node = new ChannelNode {Uri = envelope.Destination, Key = envelope.Destination.ToString()};
                node.Channel = transport.BuildChannel(node);

                _graph.Add(node);

                return node;
            }

            return destination;
        }

        public void Dispose()
        {
            _graph.Each(x => x.Channel.Dispose());
        }

        public void Start()
        {
            _transports.Each(x => x.OpenChannels(_graph));

            _graph.StartReceiving(_invoker.Value);
        }

        public ChannelNode ReplyNodeFor(ChannelNode destination)
        {
            return _graph.FirstOrDefault(x => x.Protocol() == destination.Protocol() && x.ForReplies);
        }
    }
}