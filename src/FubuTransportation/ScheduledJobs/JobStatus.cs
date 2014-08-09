using System;
using System.Diagnostics;

namespace FubuTransportation.ScheduledJobs
{
    public class JobStatus : IJobStatus
    {
        public static JobStatus For<T>(DateTimeOffset nextTime)
        {
            return new JobStatus(typeof(T), nextTime);
        }

        public JobStatus()
        {
        }

        public JobStatus(Type jobType, DateTimeOffset nextTime)
        {
            JobType = jobType.FullName;
            NextTime = nextTime;
        }

        public string JobType { get; set; }
        public DateTimeOffset? NextTime { get; set; }
        public JobExecutionRecord LastExecution { get; set; }

        protected bool Equals(JobStatus other)
        {
            return string.Equals(JobType, other.JobType) && NextTime.Equals(other.NextTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JobStatus) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((JobType != null ? JobType.GetHashCode() : 0)*397) ^ NextTime.GetHashCode();
            }
        }
    }
}