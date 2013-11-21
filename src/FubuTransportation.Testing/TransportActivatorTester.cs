using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class when_activating_the_transport_subsystem : InteractionContext<TransportActivator>
    {
        private ChannelGraph theGraph;

        protected override void beforeEach()
        {
            theGraph = MockFor<ChannelGraph>();
            Services.PartialMockTheClassUnderTest();

            ClassUnderTest.Expect(x => x.OpenChannels());

            ClassUnderTest.Activate(new IPackageInfo[0], new PackageLog());
        }


        [Test]
        public void reads_the_settings()
        {
            theGraph.AssertWasCalled(x => x.ReadSettings(MockFor<IServiceLocator>()));
        }


        [Test]
        public void should_start_the_channels()
        {
            ClassUnderTest.VerifyAllExpectations();
        }

        [Test]
        public void should_start_receiving()
        {
            theGraph.AssertWasCalled(x => x.StartReceiving(MockFor<IHandlerPipeline>()));
        }
    }


    [TestFixture]
    public class when_starting_the_subscriptions : InteractionContext<TransportActivator>
    {
        private ChannelGraph theGraph;
        private ITransport[] theTransports;

        protected override void beforeEach()
        {
            theGraph = MockFor<ChannelGraph>();
            theTransports = Services.CreateMockArrayFor<ITransport>(5);

            ClassUnderTest.OpenChannels();
        }


        [Test]
        public void starts_each_transport()
        {
            theTransports.Each(transport => transport.AssertWasCalled(x => x.OpenChannels(theGraph)));
        }


    }

    [TestFixture]
    public class when_starting_the_subscriptions_and_there_are_unknown_channels
    {
        [Test]
        public void should_throw_an_exception_listing_the_channels_that_are_missing()
        {
            var graph = new ChannelGraph();
            graph.Add(new ChannelNode
            {
                Key = "Foo:1",
                Uri = "foo://1".ToUri()
            });

            graph.Add(new ChannelNode
            {
                Key = "Foo:2",
                Uri = "foo://2".ToUri()
            });

            var subscriptions = new TransportActivator(graph, null, null,
                new ITransport[] {new FubuTransportation.InMemory.InMemoryTransport()});


            Exception<InvalidOrMissingTransportException>.ShouldBeThrownBy(subscriptions.OpenChannels);
        }
    }
}