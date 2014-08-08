using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore.Dates;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using FubuTransportation.Registration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJobs
{
    // Need to add the default job channel
    [ApplicationLevel]
    public class ScheduledJobGraph : IHandlerSource
    {
        public readonly IList<ScheduledJob> Jobs = new List<ScheduledJob>();

        public void DetermineSchedule(DateTimeOffset now, JobSchedule schedule)
        {
            throw new NotImplementedException("Not really done");

            // Make sure that all existing jobs are schedules
            Jobs.Each(x => x.Reschedule(now, schedule));

            var types = Jobs.Select(x => x.JobType).ToArray();
            schedule.RemoveObsoleteJobs(types);
        }

        public IEnumerable<HandlerCall> FindCalls()
        {
            return Jobs.Select(x => x.ToHandlerCall());
        }
    }


    public class JobExecutionRecord
    {
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Finished { get; set; }
        public bool Success { get; set; }
        public string ExceptionText { get; set; }
    }

    public interface IJobStatus
    {
        string JobType { get; }
        DateTimeOffset? NextTime { get; }

        JobExecutionRecord LastExecution { get; }
    }

    public interface IScheduleRepository
    {
        IEnumerable<JobStatus> FindJobSchedule(string nodeName);
        void PersistChanges(string nodeName, IEnumerable<JobStatus> changes, IEnumerable<JobStatus> deletions);
        void PersistChange(string nodeName, JobStatus status);

        IEnumerable<JobStatus> FindReadyToExecuteJobs(string nodeName, DateTimeOffset now);
    }

    public interface IJobScheduler
    {
        void Reschedule();
        DateTimeOffset? NextScheduledExecution { get; }
        bool IsActive { get; }
        void Activate();
        void Deactivate();
    }

    public class JobScheduler : IJobScheduler
    {
        private readonly ISystemTime _systemTime;
        private readonly IScheduleRepository _repository;
        private readonly ScheduledJobGraph _settings;
        private readonly IServiceBus _serviceBus;
        private readonly ChannelGraph _graph;
        private DefaultTimer _timer;

        public JobScheduler(ISystemTime systemTime, IScheduleRepository repository, ScheduledJobGraph settings,
            IServiceBus serviceBus, ChannelGraph graph)
        {
            _systemTime = systemTime;
            _repository = repository;
            _settings = settings;
            _serviceBus = serviceBus;
            _graph = graph;
            _timer = new DefaultTimer();
        }

        public void RunScheduledJobs()
        {
        }

        public void Reschedule()
        {
            if (!_settings.Jobs.Any()) return;

            var schedule = _repository.FindJobSchedule(_graph.Name).ToSchedule();

            _settings.DetermineSchedule(_systemTime.UtcNow(), schedule);

            // TODO -- do some logging here.
            _repository.PersistChanges(_graph.Name, schedule.Changes(), schedule.Removals());

            var nextTime = schedule.NextExecutionTime();

            ResetTimerTo(nextTime.AddSeconds(1));
        }

        public void ResetTimerTo(DateTimeOffset time)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset? NextScheduledExecution { get; private set; }
        public bool IsActive { get; private set; }

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public void Deactivate()
        {
            throw new NotImplementedException();
        }
    }

    public static class JobStatusExtensions
    {
        public static JobSchedule ToSchedule(this IEnumerable<JobStatus> statuses)
        {
            return new JobSchedule(statuses);
        }
    }

    public class JobStatus : IJobStatus
    {
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


    public class ScheduleChanges
    {
        public readonly IList<JobSchedule> Changes = new List<JobSchedule>();
        public readonly IList<JobSchedule> Removals = new List<JobSchedule>();
    }
}