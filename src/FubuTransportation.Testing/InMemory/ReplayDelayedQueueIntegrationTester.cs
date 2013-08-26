using System;
using System.Threading;
using Bottles.Services.Messaging.Tracking;
using FubuCore.Dates;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Testing.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using System.Collections.Generic;
using FubuCore;
using System.Linq;
using FubuTestingSupport;

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
        private DelayedEnvelopeProcessor theProcessor;

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

            theServiceBus.DelaySend(message1, theClock.UtcNow().AddHours(1));
            theServiceBus.DelaySend(message2, theClock.UtcNow().AddHours(1));
            theServiceBus.DelaySend(message3, theClock.UtcNow().AddHours(2));
            theServiceBus.DelaySend(message4, theClock.UtcNow().AddHours(2));

            theProcessor = runtime.Factory.Get<DelayedEnvelopeProcessor>();
        }

        [Test]
        public void things_are_received_at_the_right_times()
        {
            TestMessageRecorder.AllProcessed.Any().ShouldBeFalse();

            theProcessor.Execute();
            Thread.Sleep(2000);
            TestMessageRecorder.AllProcessed.Any().ShouldBeFalse();

            theClock.LocalNow(theClock.LocalTime().Add(61.Minutes()));
            theProcessor.Execute();

            Wait.Until(() => TestMessageRecorder.HasProcessed(message1)).ShouldBeTrue();
            Wait.Until(() => TestMessageRecorder.HasProcessed(message2)).ShouldBeTrue();

            TestMessageRecorder.HasProcessed(message3).ShouldBeFalse();
            TestMessageRecorder.HasProcessed(message4).ShouldBeFalse();

            theClock.LocalNow(theClock.LocalTime().Add(61.Minutes()));
            theProcessor.Execute();

            Wait.Until(() => TestMessageRecorder.HasProcessed(message3)).ShouldBeTrue();
            Wait.Until(() => TestMessageRecorder.HasProcessed(message4)).ShouldBeTrue();

            // If it's more than this, we got problems
            TestMessageRecorder.AllProcessed.Count().ShouldEqual(4);
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