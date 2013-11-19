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
            // TODO -- change this to a "role"
            addReplyNode(graph);
            var nodes = graph.NodesForProtocol(Protocol);
            seedQueues(nodes);

            nodes.OrderByDescending(x => x.Incoming).Each(x => x.Channel = buildChannel(x));
            
        }

        private void addReplyNode(ChannelGraph graph)
        {
            var replyNode = buildReplyChannel(graph);
            replyNode.Incoming = true;
            replyNode.Key = replyNode.Key ?? "{0}:{1}".ToFormat(Protocol, "replies");

            graph.AddReplyChannel(replyNode);
        }

        protected abstract IChannel buildChannel(ChannelNode channelNode);

        protected abstract void seedQueues(IEnumerable<ChannelNode> channels);
        protected abstract ChannelNode buildReplyChannel(ChannelGraph graph);
    }
}