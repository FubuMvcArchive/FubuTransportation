﻿using System;

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
        public JobExecutionStatus Status { get; set; }

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
}