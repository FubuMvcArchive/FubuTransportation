using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class when_activating_the_transport_subsystem : InteractionContext<TransportActivator>
    {
        private ChannelGraph theGraph;
        private ITransport[] theTransports;

        protected override void beforeEach()
        {
            theGraph = MockFor<ChannelGraph>();

            theTransports = Services.CreateMockArrayFor<ITransport>(5);
            
            ClassUnderTest.Activate(new IPackageInfo[0], new PackageLog());
        
        }

        [Test]
        public void starts_each_transport()
        {
            theTransports.Each(transport => transport.AssertWasCalled(x => x.OpenChannels(theGraph)));
        }

        [Test]
        public void reads_the_settings()
        {
            theGraph.AssertWasCalled(x => x.ReadSettings(MockFor<IServiceLocator>()));
        }

        [Test]
        public void should_start_receiving()
        {
            theGraph.AssertWasCalled(x => x.StartReceiving(MockFor<IReceiver>()));
        }
    }
}