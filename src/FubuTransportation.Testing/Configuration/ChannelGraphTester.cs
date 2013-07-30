using System;
using FubuCore;
using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class ChannelGraphTester
    {
        [Test]
        public void to_key_by_expression()
        {
            ChannelGraph.ToKey<ChannelSettings>(x => x.Outbound)
                        .ShouldEqual("Channel:Outbound");
        }

        [Test]
        public void channel_for_by_accessor()
        {
            var graph = new ChannelGraph();
            var channelNode = graph.ChannelFor<ChannelSettings>(x => x.Outbound);
            channelNode
                 .ShouldBeTheSameAs(graph.ChannelFor<ChannelSettings>(x => x.Outbound));


            channelNode.Key.ShouldEqual("Channel:Outbound");
            channelNode.SettingAddress.Name.ShouldEqual("Outbound");

        }

        [Test]
        public void reading_settings()
        {
            var channel = new ChannelSettings
            {
                Outbound = new Uri("channel://outbound"),
                Downstream = new Uri("channel://downstream")
            };

            var bus = new BusSettings
            {
                Outbound = new Uri("bus://outbound"),
                Downstream = new Uri("bus://downstream")
            };

            var services = new InMemoryServiceLocator();
            services.Add(channel);
            services.Add(bus);

            var graph = new ChannelGraph();
            graph.ChannelFor<ChannelSettings>(x => x.Outbound);
            graph.ChannelFor<ChannelSettings>(x => x.Downstream);
            graph.ChannelFor<BusSettings>(x => x.Outbound);
            graph.ChannelFor<BusSettings>(x => x.Downstream);

            graph.ReadSettings(services);

            graph.ChannelFor<ChannelSettings>(x => x.Outbound)
                 .Uri.ShouldEqual(channel.Outbound);
            graph.ChannelFor<ChannelSettings>(x => x.Downstream)
                 .Uri.ShouldEqual(channel.Downstream);
            graph.ChannelFor<BusSettings>(x => x.Outbound)
                .Uri.ShouldEqual(bus.Outbound);
            graph.ChannelFor<BusSettings>(x => x.Downstream)
                .Uri.ShouldEqual(bus.Downstream);
        }

        [Test]
        public void start_receiving()
        {
            Assert.Fail("Do.");
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }

    public class BusSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}