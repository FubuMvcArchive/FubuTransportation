using System;
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
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
    }
}