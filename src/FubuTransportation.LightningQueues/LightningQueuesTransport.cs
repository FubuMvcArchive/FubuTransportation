using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues
{
    public class LightningQueuesTransport : TransportBase, ITransport
    {
        public static readonly string DelayedQueueName = "delayed";
        public static readonly string ErrorQueueName = "errors";

        private readonly IPersistentQueues _queues;
        private readonly LightningQueueSettings _settings;
        private readonly IDelayedMessageCache<MessageId> _delayedMessages;

        public LightningQueuesTransport(IPersistentQueues queues, LightningQueueSettings settings, IDelayedMessageCache<MessageId> delayedMessages)
        {
            _queues = queues;
            _settings = settings;
            _delayedMessages = delayedMessages;
        }

        public void Dispose()
        {
            // IPersistentQueues is disposable
        }

        public override string Protocol
        {
            get { return LightningUri.Protocol; }
        }

        public IChannel BuildDestinationChannel(ChannelNode node)
        {
            // IF THIS METHOD IS CALLED, IT'LL ALWAYS BE THE REPLY QUEUE OF ANOTHER NODE
            // node.Incoming will ALWAYS be false.  We need to change these mechanics.
//            if(node.Incoming == false)
//                throw new InvalidOperationException("You can only build dynamic channels for 'Incoming' queues");

            var channel = buildChannel(node);
            _queues.CreateQueue(new LightningUri(node.Uri));

            return channel;
        }

        public IEnumerable<EnvelopeToken> ReplayDelayed(DateTime currentTime)
        {
            return _queues.ReplayDelayed(currentTime);
        }

        protected override IChannel buildChannel(ChannelNode channelNode)
        {
            return LightningQueuesChannel.Build(new LightningUri(channelNode.Uri), _queues, _delayedMessages, channelNode.Incoming);
        }

        protected override void seedQueues(ChannelNode[] channels)
        {
            _queues.Start(channels.Where(x => x.Incoming).Select(x => new LightningUri(x.Uri)));
        }

        protected override ChannelNode buildReplyChannel(ChannelGraph graph)
        {
            var port = _settings.DefaultPort;

            var nodes = graph.Where(x => x.Protocol() == Protocol);
            if (nodes.Any(x => x.Incoming))
            {
                port = nodes.Where(x => x.Incoming).Select(x => new LightningUri(x.Uri).Port).First();
            }
            else if (nodes.Any())
            {
                port = nodes.Select(x => new LightningUri(x.Uri).Port).First();
            }


            var uri = "{0}://localhost:{1}/{2}/replies".ToFormat(Protocol, port, graph.Name ?? "node").ToUri().NormalizeLocalhost();
            return new ChannelNode { Uri = uri };
        }
    }
}