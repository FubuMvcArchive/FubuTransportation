using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FubuCore.Dates;
using FubuTransportation.Configuration;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{

    // TODO -- write a simple in memory version of this.
    public interface ISchedulePersistence
    {
        IEnumerable<JobStatus> FindJobSchedule(string nodeName);
        void PersistChanges(string nodeName, IEnumerable<JobStatus> changes, IEnumerable<JobStatus> deletions);
        void PersistChange(string nodeName, JobStatus status);
    }

    public class InMemorySchedulePersistence : ISchedulePersistence
    {
        public IEnumerable<JobStatus> FindJobSchedule(string nodeName)
        {
            throw new NotImplementedException();
        }

        public void PersistChanges(string nodeName, IEnumerable<JobStatus> changes, IEnumerable<JobStatus> deletions)
        {
            throw new NotImplementedException();
        }

        public void PersistChange(string nodeName, JobStatus status)
        {
            throw new NotImplementedException();                                
        }

        public void Remove(string nodeName, JobStatus status)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<JobStatus> FindReadyToExecuteJobs(string nodeName, DateTimeOffset now)
        {
            throw new NotImplementedException();
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
        private readonly ISchedulePersistence _persistence;

        public ScheduleRepository(ChannelGraph channels, ISchedulePersistence persistence)
        {
            _channels = channels;
            _persistence = persistence;
        }

        
        public void Reschedule(Action<JobSchedule> scheduling)
        {
            var schedule = _persistence.FindJobSchedule(_channels.Name).ToSchedule();
            scheduling(schedule);

            _persistence.PersistChanges(_channels.Name, schedule.Changes(), schedule.Removals());
        }

        public void Reschedule(JobStatus status)
        {
            _persistence.PersistChange(_channels.Name, status);
        }
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
        private readonly ScheduledJobGraph _graph;
        private readonly IServiceBus _serviceBus;
        private DefaultTimer _timer;

        public JobScheduler(ISystemTime systemTime, IScheduleRepository repository, ScheduledJobGraph graph, IServiceBus serviceBus)
        {
            _systemTime = systemTime;
            _repository = repository;
            _graph = graph;
            _serviceBus = serviceBus;
            _timer = new DefaultTimer();
        }

        public void RunScheduledJobs()
        {
            throw new NotImplementedException();
        }

        public void Reschedule()
        {
            if (!_graph.Jobs.Any()) return;

            _repository.Reschedule(schedule => {
                _graph.DetermineSchedule(_systemTime.UtcNow(), schedule);
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