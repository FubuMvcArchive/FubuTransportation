using System;
using System.Security.Cryptography.X509Certificates;
using FubuTransportation.Configuration;
using FubuTransportation.Testing.Events;
using NUnit.Framework;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class FubuTransportRegistry_register_subscription_requirements_Tester
    {



        public class SubscribedRegistry : FubuTransportRegistry<BusSettings>
        {
            public SubscribedRegistry()
            {
                EnableInMemoryTransport();

                Channel(x => x.Inbound).ReadIncoming();

//                SubscribeLocally()
//                    .At(x => x.Outbound)
//                    .ToMessage<Message1>();
//
//                Subscribe(x => x.Inbound)
//                    .At(x => x.Outbound)
//                    .ToMessage<Message2>();
            }
        }

        public class BusSettings
        {
            public Uri Inbound { get; set; }
            public Uri Outbound { get; set; }
        }
    }
}