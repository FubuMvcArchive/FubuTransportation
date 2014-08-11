using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public interface IJobExecutor
    {
        Task<JobExecutionRecord> Execute<T>() where T : IJob;
        void ResetExecution<T>(IScheduledJob job, DateTimeOffset nextTime, JobExecutionRecord record);
        void Schedule<T>(IScheduledJob job, DateTimeOffset nextTime);

        DateTimeOffset Now();
    }


    public class ScheduledJobController : IDisposable
    {
        private readonly ScheduledJobGraph _jobs;
        private readonly IJobExecutor _executor;
        private readonly IJobTimer _timer;
        private readonly IScheduleRepository _repository;
        private bool _active;

        public ScheduledJobController(ScheduledJobGraph jobs, IJobExecutor executor, IJobTimer timer,
            IScheduleRepository repository)
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
                throw new NotImplementedException();
//                _jobs.DetermineSchedule(_timer.Now(), schedule);
//
//                schedule.Active().Each(status => {
//                    var job = _jobs.FindJob(status.JobType);
//                    job.Initialize(_timer, _executor, status);
//                });
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