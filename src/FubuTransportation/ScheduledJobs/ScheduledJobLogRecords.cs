using System;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public abstract class ScheduledJobRecord : LogRecord
    {
        public ScheduledJobRecord(IJob job)
        {
            JobKey = JobStatus.GetKey(job.GetType());
        }

        public string JobKey { get; set; }
    }

    public class ScheduledJobScheduled : LogRecord
    {
        public ScheduledJobScheduled(Type jobType, DateTimeOffset next)
        {
            JobKey = JobStatus.GetKey(jobType);
            ScheduledTime = next;
        }

        public string JobKey { get; set; }
        public DateTimeOffset ScheduledTime { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} Scheduled to start at {1}".ToFormat(JobKey, ScheduledTime.ToLocalTime());
        }
    }

    public class ScheduledJobStarted : ScheduledJobRecord
    {
        public ScheduledJobStarted(IJob job) : base(job)
        {
        }

        public override string ToString()
        {
            return "Scheduled job {0} started".ToFormat(JobKey);
        }
    }

    public class ScheduledJobSucceeded : ScheduledJobRecord
    {
        public ScheduledJobSucceeded(IJob job) : base(job)
        {
        }

        public override string ToString()
        {
            return "Scheduled job {0} succeeded".ToFormat(JobKey);
        }
    }

    public class ScheduledJobFailed : ScheduledJobRecord
    {
        public ScheduledJobFailed(IJob job, Exception ex) : base(job)
        {
            Exception = ex;
        }

        public Exception Exception { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} failed with exception {1}".ToFormat(JobKey, Exception);
        }
    }

    public class ScheduledJobFinished : ScheduledJobRecord
    {
        public ScheduledJobFinished(IJob job, long duration) : base(job)
        {
            Duration = duration;
        }

        public long Duration { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} finished in {1} ms".ToFormat(JobKey, Duration);
        }
    }
}