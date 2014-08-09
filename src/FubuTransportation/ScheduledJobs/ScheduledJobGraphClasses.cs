using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Reflection;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    // Need to add the default job channel
    [ApplicationLevel]
    public class ScheduledJobGraph 
    {
        public readonly IList<IScheduledJob> Jobs = new List<IScheduledJob>();
        public Accessor DefaultChannel { get; set; }

        public void DetermineSchedule(DateTimeOffset now, JobSchedule schedule)
        {
            // Make sure that all existing jobs are schedules
            Jobs.Each(x => x.Reschedule(now, schedule));

            var types = Jobs.Select(x => x.JobType).ToArray();
            schedule.RemoveObsoleteJobs(types);
        }

        public IScheduledJob FindJob(Type jobType)
        {
            return Jobs.FirstOrDefault(x => x.JobType == jobType);
        }
    }

    public class ScheduledJobRoutingRule<T> : IRoutingRule where T : IJob
    {
        public bool Matches(Type type)
        {
            return type == typeof (ExecuteScheduledJob<T>);
        }

        public string Describe()
        {
            return "Executes scheduled job: " + typeof (T).GetFullName();
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

    public interface ISchedulePersistence
    {
        IEnumerable<JobStatus> FindJobSchedule(string nodeName);
        void PersistChanges(string nodeName, IEnumerable<JobStatus> changes, IEnumerable<JobStatus> deletions);
        void PersistChange(string nodeName, JobStatus status);

        IEnumerable<JobStatus> FindReadyToExecuteJobs(string nodeName, DateTimeOffset now);
    }

    public interface IScheduleRepository
    {
        void Reschedule(Action<JobSchedule> scheduling);
        void Reschedule(JobStatus status);

        IEnumerable<JobStatus> FindReadyToExecuteJobs(DateTimeOffset now);
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
        private DefaultTimer _timer;

        public JobScheduler(ISystemTime systemTime, IScheduleRepository repository, ScheduledJobGraph settings, IServiceBus serviceBus)
        {
            _systemTime = systemTime;
            _repository = repository;
            _settings = settings;
            _serviceBus = serviceBus;
            _timer = new DefaultTimer();
        }

        public void RunScheduledJobs()
        {
            var jobs = _repository.FindReadyToExecuteJobs(_systemTime.UtcNow());
            while (jobs.Any())
            {
                var tasks = jobs.Select(job => {
                    throw new NotImplementedException();
                    return Task.Factory.StartNew(() => {
                        
                    });
                });
            }
        }

        public void Reschedule()
        {
            if (!_settings.Jobs.Any()) return;

            _repository.Reschedule(schedule => {
                _settings.DetermineSchedule(_systemTime.UtcNow(), schedule);

                var nextTime = schedule.NextExecutionTime();

                ResetTimerTo(nextTime.AddSeconds(1));
            });


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
}