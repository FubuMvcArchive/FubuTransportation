using System;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Configuration;
using System.Linq;

namespace FubuTransportation.Runtime
{
    public interface ITransport : IDisposable
    {
        void OpenChannels(ChannelGraph graph);
    }

    public abstract class TransportBase
    {
        public abstract string Protocol { get; }

        public void OpenChannels(ChannelGraph graph)
        {
            var replyNode = buildReplyChannel(graph);

            // TODO -- change this to a "role"
            replyNode.Incoming = true;
            replyNode.ForReplies = true;
            replyNode.Key = "{0}:{1}".ToFormat(Protocol, "Replies");
            graph.Add(replyNode);

            var nodes = graph.Where(x => x.Protocol() == Protocol).ToArray();
            seedQueues(nodes);

            nodes.Each(x => x.Channel = buildChannel(x));
        }

        protected abstract IChannel buildChannel(ChannelNode channelNode);

        protected abstract void seedQueues(ChannelNode[] channels);
        protected abstract ChannelNode buildReplyChannel(ChannelGraph graph);
    }
}