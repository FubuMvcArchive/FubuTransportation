using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Polling;
using FubuTransportation.Runtime;

namespace FubuTransportation.ScheduledJobs
{
    public class RescheduleRequest<T> where T : IJob
    {
        public DateTimeOffset NextTime { get; set; }

        protected bool Equals(RescheduleRequest<T> other)
        {
            return NextTime.Equals(other.NextTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RescheduleRequest<T>) obj);
        }

        public override int GetHashCode()
        {
            return NextTime.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Reschedule job {0} to {1}", typeof(T).GetFullName(), NextTime);
        }
    }

    public class ScheduledJobRunner<T> where T : IJob
    {
        private readonly T _job;
        private readonly IScheduleStatusMonitor _monitor;
        private readonly IScheduledJob<T> _scheduledJob;
        private readonly Envelope _envelope;

        public ScheduledJobRunner(T job, IScheduleStatusMonitor monitor, IScheduledJob<T> scheduledJob, Envelope envelope)
        {
            _job = job;
            _monitor = monitor;
            _scheduledJob = scheduledJob;
            _envelope = envelope;
        }


        public Task<RescheduleRequest<T>> Execute(ExecuteScheduledJob<T> request)
        {
            var tracker = _monitor.TrackJob(_envelope.Attempts, _job);
            return _scheduledJob.ToTask(_job, tracker);
        }
    }
}