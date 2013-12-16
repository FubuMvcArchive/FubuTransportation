using System;
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
            if (disabled) return;
            
            var nodes = graph.NodesForProtocol(Protocol);

            seedQueues(nodes);

            nodes.OrderByDescending(x => x.Incoming).Each(x => x.Channel = buildChannel(x));
            
            graph.AddReplyChannel(Protocol, getReplyUri(graph));
        }

        protected virtual bool disabled
        {
            get
            {
                return false;
            }
        }

        protected abstract Uri getReplyUri(ChannelGraph graph);

        protected abstract IChannel buildChannel(ChannelNode channelNode);

        protected abstract void seedQueues(IEnumerable<ChannelNode> channels);
    }
}