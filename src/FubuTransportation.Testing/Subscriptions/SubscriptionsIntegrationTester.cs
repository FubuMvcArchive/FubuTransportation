using System.Linq;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Runtime;
using FubuTransportation.Subscriptions;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class SubscriptionsIntegrationTester
    {
        private FubuRuntime runtime;
        private ISubscriptionGateway theRouter;
        private HarnessSettings settings;

        [SetUp]
        public void SetUp()
        {
            var container = new Container();
            settings = InMemoryTransport.ToInMemory<HarnessSettings>();
            container.Inject(settings);

            runtime = FubuTransport.For<RoutedRegistry>().StructureMap(container).Bootstrap();

            theRouter = runtime.Factory.Get<ISubscriptionGateway>();
        }

        [TearDown]
        public void Teardown()
        {
            runtime.Dispose();
        }

        [Test]
        public void if_destination_is_set_on_the_envelope_that_is_the_only_channel_returned()
        {
            var envelope = new Envelope
            {
                Message = new Events.Message1(),
                Destination = settings.Service4
            };

            theRouter.FindChannels(envelope).Single().Uri.ShouldEqual(settings.Service4);
        }

        [Test]
        public void can_happily_build_and_open_a_new_channel_for_a_destination()
        {
            var envelope = new Envelope
            {
                Message = new Events.Message1(),
                Destination = "memory://dynamic".ToUri()
            };

            theRouter.FindChannels(envelope).Single().Uri.ShouldEqual(envelope.Destination);
        }

        [Test]
        public void destination_is_specified_but_The_channel_does_not_exist_and_the_transport_is_unknown()
        {
            Exception<UnknownChannelException>.ShouldBeThrownBy(() => {
                var envelope = new Envelope
                {
                    Message = new Events.Message1(),
                    Destination = "unknown://uri".ToUri()
                };

                theRouter.FindChannels(envelope);
            });
        }

        [Test]
        public void use_type_rules_on_the_channel_graph_1()
        {
            var envelope = new Envelope {Message = new Events.Message1()};
            theRouter.FindChannels(envelope).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs("Harness:Service1");
        }

        [Test]
        public void use_type_rules_on_the_channel_graph_2()
        {
            var envelope = new Envelope { Message = new Events.Message2() };
            theRouter.FindChannels(envelope).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs("Harness:Service1", "Harness:Service3");
        }

        [Test]
        public void use_type_rules_on_the_channel_graph_3()
        {
            var envelope = new Envelope { Message = new Events.Message3() };
            theRouter.FindChannels(envelope).Select(x => x.Key)
                .ShouldHaveTheSameElementsAs("Harness:Service2", "Harness:Service3");
        }
    }

    public class RoutedRegistry : FubuTransportRegistry<HarnessSettings>
    {
        public RoutedRegistry()
        {
            EnableInMemoryTransport();

            Channel(x => x.Service1).AcceptsMessage<Events.Message1>();
            Channel(x => x.Service1).AcceptsMessage<Events.Message2>();

            Channel(x => x.Service2).AcceptsMessage<Events.Message3>();
            Channel(x => x.Service2).AcceptsMessage<Events.Message4>();
            
            Channel(x => x.Service3).AcceptsMessage<Events.Message2>();
            Channel(x => x.Service3).AcceptsMessage<Events.Message3>();

            Channel(x => x.Service4).ReadIncoming();

        }
    }
}