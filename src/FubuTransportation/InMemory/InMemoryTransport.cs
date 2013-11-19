using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FubuCore.Descriptions;
using FubuCore.Reflection;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuCore;
using FubuTransportation.Runtime.Delayed;

namespace FubuTransportation.InMemory
{
    [Description("A simple in memory transport suitable for automated testing or development")]
    [Title("In Memory Transport")]
    public class InMemoryTransport : TransportBase, ITransport
    {
        public void Dispose()
        {
            // nothing
        }

        public override string Protocol
        {
            get { return InMemoryChannel.Protocol; }
        }

        public IChannel BuildDestinationChannel(Uri destination)
        {
            return new InMemoryChannel(destination);
        }

        public IEnumerable<EnvelopeToken> ReplayDelayed(DateTime currentTime)
        {
            return InMemoryQueueManager.DequeueDelayedEnvelopes(currentTime);
        }

        public void ClearAll()
        {
            InMemoryQueueManager.ClearAll();
        }

        protected override IChannel buildChannel(ChannelNode channelNode)
        {
            return new InMemoryChannel(channelNode.Uri);
        }

        protected override void seedQueues(IEnumerable<ChannelNode> channels)
        {

        }

        protected override Uri getReplyUri(ChannelGraph graph)
        {
            var uri = "{0}://localhost/{1}/replies".ToFormat(Protocol, graph.Name ?? "node").ToUri();
            var replyNode = new ChannelNode
            {
                Uri = uri,
                Channel = new InMemoryChannel(uri),
                Incoming = true
            };

            replyNode.Key = replyNode.Key ?? "{0}:{1}".ToFormat(Protocol, "replies");

            graph.Add(replyNode);

            replyNode.Channel = buildChannel(replyNode);

            return uri;
        }

        public static T ToInMemory<T>() where T : new()
        {
            var type = typeof (T);
            var settings = ToInMemory(type);

            return (T) settings;
        }

        public static object ToInMemory(Type type)
        {
            var settings = Activator.CreateInstance(type);

            type.GetProperties().Where(x => x.CanWrite && x.PropertyType == typeof (Uri)).Each(prop => {
                var accessor = new SingleProperty(prop);
                var uri = "{0}://{1}/{2}".ToFormat(InMemoryChannel.Protocol, accessor.OwnerType.Name.Replace("Settings", ""),
                                                   accessor.Name).ToLower();

                accessor.SetValue(settings, new Uri(uri));
            });

            return settings;
        }
    }
}