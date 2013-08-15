using System.Collections.Generic;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class when_starting_the_subscriptions : InteractionContext<Subscriptions>
    {
        private ChannelGraph theGraph;
        private ITransport[] theTransports;

        protected override void beforeEach()
        {
            theGraph = MockFor<ChannelGraph>();
            theTransports = Services.CreateMockArrayFor<ITransport>(5);

            ClassUnderTest.Start();
        }


        [Test]
        public void starts_each_transport()
        {
            theTransports.Each(transport => RhinoMocksExtensions.AssertWasCalled<ITransport>(transport, x => x.OpenChannels(theGraph)));
        }

        [Test]
        public void should_start_receiving()
        {
            theGraph.AssertWasCalled(x => x.StartReceiving(MockFor<IMessageInvoker>()));
        }
    }
}