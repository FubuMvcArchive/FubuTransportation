using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJob
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

        // TODO -- have this return the timer
        public void Execute(ExecuteScheduledJob<T> request)
        {
            _logger.LogAndTimeExecution(_job, () => _job.Execute());
        }
    }

    public class ExecuteScheduledJob<T> where T : IJob { }
}