using System;
using System.Diagnostics;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobLogger : IScheduledJobLogger
    {
        private readonly ILogger _logger;

        public ScheduledJobLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogAndTimeExecution(IJob job, Action action)
        {
            _logger.InfoMessage(() => new ScheduledJobStarted { Description = job.ToString() });
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                action();

                _logger.InfoMessage(() => new ScheduledJobSucceeded { Description = job.ToString() });
            }
            catch (Exception ex)
            {
                _logger.Error("Scheduled job {0} failed".ToFormat(job), ex);
                _logger.InfoMessage(() => new ScheduledJobFailed
                {
                    Description = job.ToString(),
                    Exception = ex
                });

                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.InfoMessage(() => new ScheduledJobFinished
                {
                    Description = job.ToString(),
                    Duration = stopwatch.ElapsedMilliseconds
                });
            }
        }

        public void LogNextScheduledRun(IJob job, DateTimeOffset nextTime)
        {
            _logger.InfoMessage(() => new ScheduledJobScheduled
            {
                Description = job.ToString(),
                ScheduledTime = nextTime
            });
        }
    }
}