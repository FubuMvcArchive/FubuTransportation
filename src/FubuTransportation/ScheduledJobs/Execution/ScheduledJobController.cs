using System;
using System.Collections.Generic;
using FubuCore.Logging;
using FubuTransportation.Polling;
using FubuTransportation.ScheduledJobs.Persistence;

namespace FubuTransportation.ScheduledJobs.Execution
{
    // TODO -- add LOTS of logging
    // TODO -- more state tracing!
    public interface IScheduledJobController
    {
        void Activate();
        bool IsActive();
        void Deactivate();
        void Reschedule<T>(RescheduleRequest<T> request) where T : IJob;
    }

    public class ScheduledJobController : IDisposable, IJobExecutor, IScheduledJobController
    {
        private readonly ScheduledJobGraph _jobs;
        private readonly IJobTimer _timer;
        private readonly IScheduleStatusMonitor _statusMonitor;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _logger;
        private bool _active;

        public ScheduledJobController(
            ScheduledJobGraph jobs,
            IJobTimer timer,
            IScheduleStatusMonitor statusMonitor,
            IServiceBus serviceBus,
            ILogger logger)
        {
            _jobs = jobs;
            _timer = timer;
            _statusMonitor = statusMonitor;
            _serviceBus = serviceBus;
            _logger = logger;
        }

        public void Activate()
        {
            _timer.ClearAll();

            _statusMonitor.Persist(schedule => {
                _jobs.DetermineSchedule(_timer.Now(), this, schedule);

                schedule.Active().Each(status => {
                    var job = _jobs.FindJob(status.JobType);
                    job.Initialize(_timer.Now(), this, schedule);
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

        public void Reschedule<T>(RescheduleRequest<T> request) where T : IJob
        {
            var job = _jobs.FindJob<T>();
            if (job != null)
            {
                Schedule(job, request.NextTime);
            }
        }

        public void Dispose()
        {
            _active = false;
            _timer.ClearAll();
        }


        public void Execute<T>(TimeSpan timeout) where T : IJob
        {
            _serviceBus.Send(new ExecuteScheduledJob<T>());
        }

        public void Schedule<T>(IScheduledJob<T> job, DateTimeOffset nextTime) where T : IJob
        {
            _statusMonitor.MarkScheduled<T>(nextTime);
            _timer.Schedule(typeof (T), nextTime, () => job.Execute(this));

            _logger.InfoMessage(() => new ScheduledJobScheduled(typeof (T), nextTime));
        }
    }
}