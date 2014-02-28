using System;
using System.Data;
using FubuTransportation;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Serializers;
using StructureMap.Diagnostics;

namespace DiagnosticsHarness
{
    public class HarnessRegistry : FubuTransportRegistry<HarnessSettings>
    {
        public HarnessRegistry()
        {
            EnableInMemoryTransport();

            // TODO -- publish everything option in the FI?
            Channel(x => x.Channel).ReadIncoming().AcceptsMessages(x => true).DefaultSerializer<XmlMessageSerializer>();

            Global.Policy<ErrorHandlingPolicy>();

            SubscribeLocally().ToSource(x => x.Publisher)
                .ToMessage<NumberMessage>();
        }
    }

    public class ErrorHandlingPolicy : HandlerChainPolicy
    {
        public override void Configure(HandlerChain chain)
        {
            chain.MaximumAttempts = 5;
            chain.OnException<TimeoutException>().Retry();
            chain.OnException<DBConcurrencyException>().Retry();
        }
    }
}