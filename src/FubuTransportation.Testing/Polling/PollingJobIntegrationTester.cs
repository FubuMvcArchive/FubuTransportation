using System;
using System.Collections.Generic;
using System.Threading;
using FubuCore.Logging;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using NUnit.Framework;
using StructureMap;
using FubuMVC.StructureMap;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class PollingJobIntegrationTester
    {
        private Container container;

        [TestFixtureSetUp]
        public void SetUp()
        {
            OneJob.Executed = TwoJob.Executed = ThreeJob.Executed = 0;

            
            container = new Container();
                
            FubuTransport.For<PollingRegistry>().StructureMap(container)
                                       .Bootstrap();


            Wait.Until(() => ThreeJob.Executed > 10, timeoutInMilliseconds:60000);
        }

        [Test]
        public void there_are_polling_jobs_registered()
        {
            // The polling job for delayed messages & one for the expired listeners are registered by default.
            var pollingJobs = container.GetInstance<IPollingJobs>();
            pollingJobs.Count()
                     .ShouldEqual(5);
        }

        [Test]
        public void should_have_executed_all_the_jobs_several_times()
        {
            OneJob.Executed.ShouldBeGreaterThan(10);

            TwoJob.Executed.ShouldBeGreaterThan(10);

            ThreeJob.Executed.ShouldBeGreaterThan(10);
        }

        [Test]
        public void should_have_executed_one_more_than_two_and_two_more_than_three_because_of_the_polling_interval_differences()
        {
            OneJob.Executed.ShouldBeGreaterThan(TwoJob.Executed);
            TwoJob.Executed.ShouldBeGreaterThan(ThreeJob.Executed);
        }
    }

    public class PollingRegistry : FubuTransportRegistry
    {
        public PollingRegistry()
        {
            Polling.RunJob<OneJob>().ScheduledAtInterval<PollingSettings>(x => x.OneInterval);
            Polling.RunJob<TwoJob>().ScheduledAtInterval<PollingSettings>(x => x.TwoInterval);
            Polling.RunJob<ThreeJob>().ScheduledAtInterval<PollingSettings>(x => x.ThreeInterval);

            Services(x => x.ReplaceService<IPollingJobLogger, RecordingPollingJobLogger>());
        }
    }

    public class PollingSettings
    {
        public PollingSettings()
        {
            OneInterval = 100;
            TwoInterval = 200;
            ThreeInterval = 300;
        }

        public double OneInterval { get; set; }
        public double TwoInterval { get; set; }
        public double ThreeInterval { get; set; }
    }

    public class OneJob : IJob
    {
        public static int Executed = 0;

        public void Execute()
        {
            Executed++;
        }
    }

    public class TwoJob : IJob
    {
        public static int Executed = 0;

        public void Execute()
        {
            Executed++;
        }
    }

    public class ThreeJob : IJob
    {
        public static int Executed = 0;

        public void Execute()
        {
            Executed++;
        }
    }

    public class RecordingPollingJobLogger : IPollingJobLogger
    {
        public readonly IList<Type> Stopped = new List<Type>(); 
        public readonly IList<IJob> Started = new List<IJob>(); 
        public readonly IList<IJob> Succeeded = new List<IJob>(); 

        public void Stopping(Type jobType)
        {
            Stopped.Add(jobType);
        }

        public void Starting(IJob job)
        {
            Started.Add(job);
        }

        public void Successful(IJob job)
        {
            Succeeded.Add(job);
        }

        public void Failed(IJob job, Exception ex)
        {
            Assert.Fail("Got an exception for {0}\n{1}", job, ex);
        }

        public void FailedToSchedule(Type jobType, Exception exception)
        {
            Assert.Fail("Failed to schedule {0}\n{1}", jobType, exception);
        }
    }

}