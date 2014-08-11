using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    // TODO -- add LOTS of logging
    // TODO -- more state tracing!
    public interface IScheduledJobController
    {
        void Activate();
        bool IsActive();
        void Deactivate();
    }

    public class ScheduledJobController : IDisposable, IJobExecutor, IScheduledJobController
    {
        private readonly ScheduledJobGraph _jobs;
        private readonly IJobTimer _timer;
        private readonly IScheduleRepository _repository;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _logger;
        private bool _active;

        public ScheduledJobController(
            ScheduledJobGraph jobs,
            IJobTimer timer,
            IScheduleRepository repository,
            IServiceBus serviceBus,
            ILogger logger)
        {
            _jobs = jobs;
            _timer = timer;
            _repository = repository;
            _serviceBus = serviceBus;
            _logger = logger;
        }

        public void Activate()
        {
            _timer.ClearAll();

            _repository.Persist(schedule => {
                _jobs.DetermineSchedule(this, schedule);

                schedule.Active().Each(status => {
                    var job = _jobs.FindJob(status.JobType);
                    job.Initialize(this, schedule);
                });
            });

            _active = true;
        }

        public bool IsActive()
        {
            return _active;
        }

        public void Deactivate()
        {
            Dispose();
        }

        public void Dispose()
        {
            _active = false;
            _timer.ClearAll();
        }


        public Task<JobExecutionRecord> Execute<T>() where T : IJob
        {
            return _serviceBus.Request<JobExecutionRecord>(new ExecuteScheduledJob<T>());
        }

        public void Reschedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime, JobExecutionRecord record) where T : IJob
        {
            _repository.Persist(new JobStatus(typeof(T), nextTime)
            {
                LastExecution = record
            });

            Schedule(job, nextTime);
        }

        public void Schedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime) where T : IJob
        {
            _timer.Schedule(typeof(T), nextTime, () => job.Execute(this));
        }

        public DateTimeOffset Now()
        {
            return _timer.Now();
        }
    }
}