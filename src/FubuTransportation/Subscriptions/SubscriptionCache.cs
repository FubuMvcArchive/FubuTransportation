using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    public class SubscriptionCache : ISubscriptionCache, IDisposable
    {
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<ITransport> _transports;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly IDictionary<Type, IList<ChannelNode>> _routes = new Dictionary<Type, IList<ChannelNode>>();
        private readonly IDictionary<Uri, ChannelNode> _volatileNodes = new Dictionary<Uri, ChannelNode>(); 

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
                var uri = envelope.Destination;
                var destination = findDestination(uri);

                return new ChannelNode[] {destination};
            }

            var inputType = envelope.Message.GetType();
            return _lock.MaybeWrite(() => _routes[inputType], () => !_routes.ContainsKey(inputType), () => {
                var nodes = FindSubscribingChannelsFor(inputType);
                _routes.Add(inputType, new List<ChannelNode>(nodes));
            });
        }

        public IEnumerable<ChannelNode> FindSubscribingChannelsFor(Type inputType)
        {
            return _graph.Where(x => x.Publishes(inputType));
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