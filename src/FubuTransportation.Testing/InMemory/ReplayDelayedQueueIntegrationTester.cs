using System;
using Bottles.Services.Messaging.Tracking;
using FubuCore.Dates;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Testing.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using System.Collections.Generic;
using FubuCore;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class ReplayDelayedQueueIntegrationTester
    {
        private IServiceBus theServiceBus;
        private SettableClock theClock;
        private OneMessage message1;
        private OneMessage message2;
        private OneMessage message3;
        private OneMessage message4;

        [TestFixtureSetUp]
        public void SetUp()
        {
            // Need to do something about this.  Little ridiculous
            FubuTransport.SetupForInMemoryTesting();
            TestMessageRecorder.Clear();
            MessageHistory.ClearAll();
            InMemoryQueueManager.ClearAll();

            var runtime = FubuTransport.For<DelayedRegistry>().StructureMap(new Container())
                                       .Bootstrap();

            // Disable polling!
            runtime.Factory.Get<IPollingJobs>().Each(x => x.Stop());

            theServiceBus = runtime.Factory.Get<IServiceBus>();

            theClock = runtime.Factory.Get<ISystemTime>().As<SettableClock>();

            message1 = new OneMessage();
            message2 = new OneMessage();
            message3 = new OneMessage();
            message4 = new OneMessage();

            throw new NotImplementedException();

//            theServiceBus.DelaySend(message1);
//            theServiceBus.DelaySend(message2);
//            theServiceBus.DelaySend(message3);
//            theServiceBus.DelaySend(message4);
        }

        [Test]
        public void nothing_should_be_received_yet()
        {

        }


        [TestFixtureTearDown]
        public void TearDown()
        {
            FubuTransport.Reset();
        }
    }

    public class DelayedRegistry : FubuTransportRegistry<BusSettings>
    {
        public DelayedRegistry()
        {
            Services(x => x.ReplaceService<ISystemTime>(new SettableClock()));
            Handlers.Include<SimpleHandler<OneMessage>>();
            Channel(x => x.Downstream).ReadIncoming().PublishesMessagesInAssemblyContainingType<OneMessage>();
        }
    }
}