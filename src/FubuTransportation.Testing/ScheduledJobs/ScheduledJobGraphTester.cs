using System;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class when_scheduling_jobs
    {
        private JobSchedule theSchedule;
        private ScheduledJobGraph theGraph;

        [SetUp]
        public void SetUp()
        {
            theSchedule = new JobSchedule(new []
            {
                JobStatus.For<AJob>(DateTime.Today), 
                JobStatus.For<BJob>(DateTime.Today.AddHours(1)), 
                JobStatus.For<CJob>(DateTime.Today.AddHours(2)), 
            });

            theGraph = new ScheduledJobGraph();
            theGraph.Jobs.Add(new ScheduledJob<BJob>(new DummyScheduleRule(DateTime.Today.AddHours(1))));
            theGraph.Jobs.Add(new ScheduledJob<CJob>(new DummyScheduleRule(DateTime.Today.AddHours(3))));
            theGraph.Jobs.Add(new ScheduledJob<DJob>(new DummyScheduleRule(DateTime.Today.AddHours(4))));
            theGraph.Jobs.Add(new ScheduledJob<EJob>(new DummyScheduleRule(DateTime.Today.AddHours(5))));
        
            // not that worried about pushing the time around
            theGraph.DetermineSchedule(DateTimeOffset.Now, theSchedule);
        }

        [Test]
        public void changes_the_jobs_that_are_already_scheduled_correcting_where_necessary()
        {
            theSchedule.Find(typeof (CJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(3));
            theSchedule.Find(typeof(BJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(1));
        }

        [Test]
        public void schedules_new_jobs()
        {
            theSchedule.Find(typeof(DJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(4));
            theSchedule.Find(typeof(EJob)).NextTime.ShouldEqual((DateTimeOffset)DateTime.Today.AddHours(5));
        }

        [Test]
        public void tracks_the_jobs_that_have_changed_or_been_added()
        {
            theSchedule.Changes().Select(x => x.JobType)
                .ShouldHaveTheSameElementsAs(typeof(CJob).FullName, typeof(DJob).FullName, typeof(EJob).FullName);
        }

        [Test]
        public void removes_obsolete_jobs()
        {
            theSchedule.Removals().Select(x => x.JobType)
                .ShouldHaveTheSameElementsAs(typeof(AJob).FullName);
        }
    }

    public class DJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    public class EJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}