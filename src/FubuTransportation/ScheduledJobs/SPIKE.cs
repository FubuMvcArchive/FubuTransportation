using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public interface IJobExecutor
    {
        Task<JobStatus> Execute<T>(IScheduledJob<T> job) where T : IJob;
    }

    public class JobExecutor : IJobExecutor
    {
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;

        public JobExecutor(IServiceBus serviceBus, ILogger logger, ISystemTime systemTime)
        {
            _serviceBus = serviceBus;
            _logger = logger;
            _systemTime = systemTime;
        }

        public Task<JobStatus> Execute<T>(IScheduledJob<T> job) where T : IJob
        {
            var task = _serviceBus.Request<JobExecutionRecord>(new ExecuteScheduledJob<T>());

            // TODO -- need to implement the retry capabilities
            // TODO -- track attempts here?
            return task.ContinueWith(parent => {
                // TODO -- log reschedules and sending
                return job.ToNewJobStatus(parent.Result, _systemTime.UtcNow());
            });

            // TODO -- totally trap all errors
        }
    }

    public class ScheduledJobController : IDisposable
    {
        private readonly ScheduledJobGraph _jobs;
        private readonly IJobExecutor _executor;
        private readonly IJobTimer _timer;
        private readonly IScheduleRepository _repository;
        private bool _active;

        public ScheduledJobController(ScheduledJobGraph jobs, IJobExecutor executor, IJobTimer timer, IScheduleRepository repository)
        {
            _jobs = jobs;
            _executor = executor;
            _timer = timer;
            _repository = repository;
        }

        public void RescheduleAll()
        {
            _timer.ClearAll();

            _repository.Persist(schedule => {
                _jobs.DetermineSchedule(_timer.Now(), schedule);

                schedule.Active().Each(status => {
                    var job = _jobs.FindJob(status.JobType);
                    job.RegisterJob(_timer, _executor, status);
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
    }
}