using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Reflection;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuCore;
using FubuTransportation.Runtime.Delayed;

namespace FubuTransportation.InMemory
{
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

        public IChannel BuildChannel(ChannelNode node)
        {
            return new InMemoryChannel(node);
        }

        public IEnumerable<Envelope> ReplayDelayed(DateTime currentTime)
        {
            throw new NotImplementedException();
        }

        protected override IChannel buildChannel(ChannelNode channelNode)
        {
            return new InMemoryChannel(channelNode);
        }

        protected override void seedQueues(ChannelNode[] channels)
        {
            // no-op
        }

        protected override ChannelNode buildReplyChannel(ChannelGraph graph)
        {
            var uri = "{0}://localhost/{1}/replies".ToFormat(Protocol, graph.Name ?? "node").ToUri();
            return new ChannelNode{Uri = uri};
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