using System;
using FubuCore;
using FubuCore.Dates;
using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs;
using NUnit.Framework;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class JobStatusTester
    {
        [Test]
        public void to_status_without_any_attribute()
        {
            var status = new JobStatus(typeof (AJob), DateTime.Today)
            {
                NextTime = DateTime.Today.AddHours(1),
                Active = true,
                LastExecution = new JobExecutionRecord
                {
                    Duration = 123,
                    ExceptionText = null,
                    Finished = DateTime.Today.AddHours(-1),
                    Success = true,
                    
                }
            };

            var dto = status.ToDTO("foo");
            dto.NodeName.ShouldEqual("foo");
            dto.JobKey.ShouldEqual("AJob");
            dto.LastExecution.ShouldEqual(status.LastExecution);
            dto.NextTime.ShouldEqual(status.NextTime);
            dto.Active.ShouldBeTrue();
        }

        [Test]
        public void to_status_with_attribute_on_job_type()
        {
            var status = new JobStatus(typeof(DecoratedJob), DateTime.Today)
            {
                Active = false,
                NextTime = DateTime.Today.AddHours(1),
                LastExecution = new JobExecutionRecord
                {
                    Duration = 123,
                    ExceptionText = null,
                    Finished = DateTime.Today.AddHours(-1),
                    Success = true
                }
            };

            var dto = status.ToDTO("foo");
            dto.NodeName.ShouldEqual("foo");
            dto.JobKey.ShouldEqual("Special");
            dto.LastExecution.ShouldEqual(status.LastExecution);
            dto.NextTime.ShouldEqual(status.NextTime);
            dto.Active.ShouldBeFalse();
        }
    }

    [JobKey("Special")]
    public class DecoratedJob : IJob
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}