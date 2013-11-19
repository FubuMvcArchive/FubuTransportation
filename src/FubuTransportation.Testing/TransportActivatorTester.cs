using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class when_activating_the_transport_subsystem : InteractionContext<TransportActivator>
    {
        private ChannelGraph theGraph;

        protected override void beforeEach()
        {
            theGraph = MockFor<ChannelGraph>();
            ClassUnderTest.Activate(new IPackageInfo[0], new PackageLog());
        }


        [Test]
        public void reads_the_settings()
        {
            theGraph.AssertWasCalled(x => x.ReadSettings(MockFor<IServiceLocator>()));
        }



        [Test]
        public void should_start_the_subscriptions()
        {
            MockFor<ISubscriptionGateway>().AssertWasCalled(x => x.Start());
        }
    }

}