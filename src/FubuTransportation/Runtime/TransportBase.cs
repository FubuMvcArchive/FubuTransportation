using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public abstract class TransportBase
    {
        public abstract string Protocol { get; }

        public void OpenChannels(ChannelGraph graph)
        {
            ReplyNode = buildReplyChannel(graph);

            // TODO -- change this to a "role"
            ReplyNode.Incoming = true;
            ReplyNode.ForReplies = true;
            ReplyNode.Key = "{0}:{1}".ToFormat(Protocol, "replies");
            graph.Add(ReplyNode);

            var nodes = graph.Where(x => x.Protocol() == Protocol).ToArray();
            seedQueues(nodes);

            nodes.OrderByDescending(x => x.Incoming).Each(x => x.Channel = buildChannel(x));
        }

        public ChannelNode ReplyNode { get; protected set; }

        protected abstract IChannel buildChannel(ChannelNode channelNode);

        protected abstract void seedQueues(ChannelNode[] channels);
        protected abstract ChannelNode buildReplyChannel(ChannelGraph graph);
    }
}