using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.LightningQueues
{
    public class LightningQueuesTransport : TransportBase, ITransport
    {
        public static readonly string DelayedQueueName = "delayed";

        private readonly IPersistentQueues _queues;
        private readonly LightningQueueSettings _settings;

        public LightningQueuesTransport(IPersistentQueues queues, LightningQueueSettings settings)
        {
            _queues = queues;
            _settings = settings;
        }

        public void Dispose()
        {
            // IPersistentQueues is disposable
        }

        public override string Protocol
        {
            get { return LightningUri.Protocol; }
        }

        public IChannel BuildChannel(ChannelNode node)
        {
            var channel = buildChannel(node);
            _queues.CreateQueue(new LightningUri(node.Uri));

            return channel;
        }

        public IEnumerable<Envelope> ReplayDelayed(DateTime currentTime)
        {
            return _queues.ReplayDelayed(currentTime);
        }

        protected override IChannel buildChannel(ChannelNode channelNode)
        {
            return LightningQueuesChannel.Build(new LightningUri(channelNode.Uri), _queues);
        }

        protected override void seedQueues(ChannelNode[] channels)
        {
            _queues.Start(channels.Select(x => new LightningUri(x.Uri)));
        }

        protected override ChannelNode buildReplyChannel(ChannelGraph graph)
        {
            var uri = "{0}://localhost:{1}/{2}/replies".ToFormat(Protocol, _settings.DefaultPort,graph.Name ?? "node").ToUri().NormalizeLocalhost();
            return new ChannelNode { Uri = uri };
        }
    }
}