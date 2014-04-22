using FubuTransportation.Configuration;
using FubuTransportation.Serenity.Samples.SystemUnderTest.Subscriptions;

namespace FubuTransportation.Serenity.Samples.Setup
{
    public class ClientRegistry : FubuTransportRegistry<MultipleEndpointsSettings>
    {
        public ClientRegistry()
        {
            Channel(x => x.Client)
                .AcceptsMessage<MessageForClient>()
                .ReadIncoming();
        }
    }

    public class AnotherServiceRegistry : FubuTransportRegistry<MultipleEndpointsSettings>
    {
        public AnotherServiceRegistry()
        {
            Channel(x => x.AnotherService)
                .ReadIncoming();

            SubscribeLocally()
                .ToSource(x => x.Service)
                .ToMessage<PublishedEvent>();
        }
    }
}