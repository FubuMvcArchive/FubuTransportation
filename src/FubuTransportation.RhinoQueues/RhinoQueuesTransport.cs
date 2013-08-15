using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesTransport : TransportBase, ITransport
    {
        private readonly IPersistentQueues _queues;
        private readonly RhinoQueueSettings _settings;


        public RhinoQueuesTransport(IPersistentQueues queues, RhinoQueueSettings settings)
        {
            _queues = queues;
            _settings = settings;
        }

        public void Dispose()
        {
            // IPersistentQueues is disposable
        }

        // TODO -- needs hard integration tests
//        public void OpenChannels(ChannelGraph graph)
//        {
//            ChannelNode[] rhinoChannels = graph.Where(x => x.Protocol() == RhinoUri.Protocol).ToArray();
//
//            _queues.Start(rhinoChannels.Select(x => new RhinoUri(x.Uri)));
//
//            rhinoChannels.Each(node => { node.Channel = RhinoQueuesChannel.Build(new RhinoUri(node.Uri), _queues); });
//        }

        public override string Protocol
        {
            get { return RhinoUri.Protocol; }
        }

        public IChannel BuildChannel(ChannelNode node)
        {
            var channel = buildChannel(node);
            _queues.CreateQueue(new RhinoUri(node.Uri));

            return channel;
        }

        protected override IChannel buildChannel(ChannelNode channelNode)
        {
            return RhinoQueuesChannel.Build(new RhinoUri(channelNode.Uri), _queues);
        }

        protected override void seedQueues(ChannelNode[] channels)
        {
            _queues.Start(channels.Select(x => new RhinoUri(x.Uri)));
        }

        protected override ChannelNode buildReplyChannel(ChannelGraph graph)
        {
            var uri = "{0}://localhost:{1}/{2}/replies".ToFormat(Protocol, _settings.DefaultPort,graph.Name ?? "node").ToUri().NormalizeLocalhost();
            return new ChannelNode { Uri = uri };
        }
    }
}