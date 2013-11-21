using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Subscriptions
{
    public class SubscriptionCache : ISubscriptionCache, IDisposable
    {
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<ITransport> _transports;

        public SubscriptionCache(ChannelGraph graph, IEnumerable<ITransport> transports)
        {
            if (!transports.Any())
            {
                throw new Exception(
                    "No transports are registered.  FubuTransportation cannot function without at least one ITransport");
            }

            _graph = graph;
            _transports = transports;
        }

        public IEnumerable<ChannelNode> FindDestinationChannels(Envelope envelope)
        {
            if (envelope.Destination != null)
            {
                var destination = findDestination(envelope);

                return new ChannelNode[] {destination};
            }

            // TODO -- gets a LOT more sophisticated later
            var inputType = envelope.Message.GetType();
            return _graph.Where(c => c.Rules.Any(x => x.Matches(inputType)));
        }

        private ChannelNode findDestination(Envelope envelope)
        {
            var uri = envelope.Destination;

            return findDestination(uri);
        }

        private ChannelNode findDestination(Uri uri)
        {
            var destination = _graph.FirstOrDefault(x => x.Uri == uri);
            if (destination == null)
            {
                var transport = _transports.FirstOrDefault(x => x.Protocol == uri.Scheme);
                if (transport == null)
                {
                    throw new UnknownChannelException(uri);
                }

                var node = new ChannelNode {Uri = uri, Key = uri.ToString()};
                node.Channel = transport.BuildDestinationChannel(node.Uri);

                return node;
            }

            return destination;
        }

        public void Dispose()
        {
            _graph.Dispose();
        }


        public Uri ReplyUriFor(ChannelNode destination)
        {
            return _graph.ReplyChannelFor(destination.Protocol());
        }

        public void ReloadSubscriptions()
        {
            throw new NotImplementedException();
        }
    }
}