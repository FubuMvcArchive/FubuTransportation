using System;
using FubuTransportation.Configuration;
using FubuTransportation.Serenity.Samples.SystemUnderTest.Subscriptions;

namespace FubuTransportation.Serenity.Samples.SystemUnderTest
{
    public class TestRegistry : FubuTransportRegistry<TestSettings>
    {
        public TestRegistry()
        {
            Channel(x => x.SystemUnderTest)
                .AcceptsMessage<SomeCommand>()
                .AcceptsMessage<TestMessage>()
                .ReadIncoming();
        }
    }

    public class TestSettings
    {
        public Uri SystemUnderTest { get; set; }
    }
}