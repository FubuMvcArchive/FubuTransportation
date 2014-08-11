using System.Diagnostics;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobRunner<T> where T : IJob
    {
        private readonly T _job;
        private readonly IScheduledJobLogger _logger;

        public ScheduledJobRunner(T job, IScheduledJobLogger logger)
        {
            _job = job;
            _logger = logger;
        }

        public JobExecutionRecord Execute(ExecuteScheduledJob<T> request)
        {
            _logger.LogAndTimeExecution(_job, () => _job.Execute());

            // TODO -- do this for realsies
            return new JobExecutionRecord{Success = true};
        }
    }

    public class ExecuteScheduledJob<T> where T : IJob { }
}