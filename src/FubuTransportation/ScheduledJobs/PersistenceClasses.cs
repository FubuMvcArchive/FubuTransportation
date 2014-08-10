using System;
using System.Web.Caching;
using FubuTransportation.Configuration;

namespace FubuTransportation.ScheduledJobs
{
    public class JobStatusDTO
    {
        public string JobKey { get; set; }

        public string Id
        {
            get
            {
                return NodeName + "/" + JobKey;
            }
        }

        public DateTimeOffset? NextTime { get; set; }
        public JobExecutionRecord LastExecution { get; set; }
        public string NodeName { get; set; }
        public bool Active { get; set; }

        protected bool Equals(JobStatusDTO other)
        {
            return string.Equals(NodeName, other.NodeName) && string.Equals(JobKey, other.JobKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JobStatusDTO) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NodeName != null ? NodeName.GetHashCode() : 0)*397) ^ (JobKey != null ? JobKey.GetHashCode() : 0);
            }
        }
    }


    public interface IScheduleRepository
    {
        void Reschedule(Action<JobSchedule> scheduling);
        void Reschedule(JobStatus status);
    }

    // TODO -- register this thing
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ChannelGraph _channels;
        private readonly ScheduledJobGraph _jobs;
        private readonly ISchedulePersistence _persistence;

        public ScheduleRepository(ChannelGraph channels, ScheduledJobGraph jobs, ISchedulePersistence persistence)
        {
            _channels = channels;
            _jobs = jobs;
            _persistence = persistence;
        }


        public void Reschedule(Action<JobSchedule> scheduling)
        {
            throw new NotImplementedException();
        }

        public void Reschedule(JobStatus status)
        {
            throw new NotImplementedException();
        }
    }


}