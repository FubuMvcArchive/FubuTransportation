using System.Collections.Generic;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
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
            theGraph.AssertWasCalled(x => x.StartReceiving(MockFor<IHandlerPipeline>()));
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

            var subscriptions = new Subscriptions(graph, () => null, new ITransport[]{new InMemoryTransport()});


            Exception<InvalidOrMissingTransportException>.ShouldBeThrownBy(() => {
                subscriptions.Start();
            });
        }
    }
}