using FubuCore.Dates;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJob
{
    public interface IScheduledJobInitializer
    {
        void StartUp();
    }

    public class ScheduledJobInitializer<TJob> : IScheduledJobInitializer where TJob : IJob
    {
        private readonly TJob _job;
        private readonly IJobScheduler _jobScheduler;
        private readonly IServiceBus _bus;
        private readonly ISystemTime _systemTime;
        private readonly IScheduledJobLogger _logger;

        public ScheduledJobInitializer(TJob job, IJobScheduler jobScheduler, IServiceBus bus,
                                       ISystemTime systemTime, IScheduledJobLogger logger)
        {
            _job = job;
            _jobScheduler = jobScheduler;
            _bus = bus;
            _systemTime = systemTime;
            _logger = logger;
        }

        public void StartUp()
        {
            // TODO: Check if already scheduled? Bully algorithm does this instead?

            var nextTime = _jobScheduler.ScheduleNextTime(_systemTime.UtcNow());
            _logger.LogNextScheduledRun(_job, nextTime);
            // TODO: Log when job fails to schedule?

            // TODO: Send delayed bus message to scheduled job?
            _bus.DelaySend(new ExecuteScheduledJob<TJob>(), nextTime.UtcDateTime);
        }
    }
}