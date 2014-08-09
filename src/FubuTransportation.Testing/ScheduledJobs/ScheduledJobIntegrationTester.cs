using System;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class ScheduledJobIntegrationTester
    {
        private Container container;
        private FubuRuntime theRuntime;

        [TestFixtureSetUp]
        public void SetUp()
        {
            AJob.Reset();
            BJob.Reset();
            CJob.Reset();

            container = new Container();
            theRuntime = FubuTransport.For<ScheduledJobRegistry>()
                .StructureMap(container)
                .Bootstrap();
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            theRuntime.Dispose();
        }

        [Test]
        public void the_scheduled_job_chains_are_present()
        {
            var graph = theRuntime.Factory.Get<BehaviorGraph>();
            var chains = graph.Behaviors.Where(x => x.InputType() != null && x.InputType().Closes(typeof (ExecuteScheduledJob<>)));

            chains.ShouldHaveCount(3);
        }

    }

    public class ScheduledJobRegistry : FubuTransportRegistry<BusSettings>
    {
        public ScheduledJobRegistry()
        {
            EnableInMemoryTransport();

            ScheduledJob.RunJob<AJob>().ScheduledBy<DummyScheduleRule>();
            ScheduledJob.RunJob<BJob>().ScheduledBy<DummyScheduleRule>();
            ScheduledJob.RunJob<CJob>().ScheduledBy<DummyScheduleRule>();
        }
    }

    public class AJob : IJob
    {
        public static int Executed = 0;

        public void Execute() { ++Executed; }
        public static void Reset() { Executed = 0; }
    }

    public class BJob : IJob
    {
        public static int Executed = 0;

        public void Execute() { ++Executed; }
        public static void Reset() { Executed = 0; }
    }

    public class CJob : IJob
    {
        public static int Executed = 0;

        public void Execute() { ++Executed; }
        public static void Reset() { Executed = 0; }
    }

    public class DummyScheduleRule : IScheduleRule
    {
        private readonly DateTimeOffset _nextTime;

        public DummyScheduleRule()
        {
            _nextTime = DateTimeOffset.Now;
        }

        public DummyScheduleRule(DateTimeOffset nextTime)
        {
            _nextTime = nextTime;
        }

        public DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime)
        {
            return _nextTime;
        }
    }
}
