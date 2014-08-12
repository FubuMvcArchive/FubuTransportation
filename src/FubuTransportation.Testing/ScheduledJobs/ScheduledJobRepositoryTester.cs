﻿using System;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class ScheduledJobRepositoryTester
    {
        private JobStatusDTO foo1;
        private JobStatusDTO foo2;
        private JobStatusDTO foo3;
        private JobStatusDTO bar1;
        private JobStatusDTO bar2;
        private InMemorySchedulePersistence thePersistence;
        private RecordingLogger theLogger;
        private ScheduleRepository theRepository;

        [SetUp]
        public void SetUp()
        {
            foo1 = new JobStatusDTO { JobKey = "1", NodeName = "foo" };
            foo2 = new JobStatusDTO { JobKey = "2", NodeName = "foo" };
            foo3 = new JobStatusDTO { JobKey = "3", NodeName = "foo" };
            bar1 = new JobStatusDTO { JobKey = "1", NodeName = "bar" };
            bar2 = new JobStatusDTO { JobKey = "2", NodeName = "bar" };

            thePersistence = new InMemorySchedulePersistence();
            thePersistence.Persist(new[] { foo1, foo2, foo3, bar1, bar2 });

            theRepository = new ScheduleRepository(new ChannelGraph {Name = "foo"}, new ScheduledJobGraph(), thePersistence);

        }

        [Test]
        public void mark_scheduled_persistence()
        {
            var next = (DateTimeOffset)DateTime.Today;
            theRepository.MarkScheduled<FooJob1>(next);

            foo1.Status.ShouldEqual(JobExecutionStatus.Scheduled);
            foo1.NextTime.ShouldEqual(next);
        }

        [Test]
        public void mark_executing_persistence()
        {
            theRepository.MarkExecuting<FooJob1>();

            foo1.Status.ShouldEqual(JobExecutionStatus.Executing);
        }

        [Test]
        public void mark_completion_persistence()
        {
            var record = new JobExecutionRecord();
            theRepository.MarkCompletion<FooJob1>(record);

            foo1.Status.ShouldEqual(JobExecutionStatus.Completed);
            foo1.LastExecution.ShouldBeTheSameAs(record);
        }


        [JobKey("1")]
        public class FooJob1 : IJob
        {
            public void Execute()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}